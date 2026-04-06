using Coolzo.Application.Common.Interfaces;
using Coolzo.Application.Common.Services;
using Coolzo.Contracts.Responses.Billing;
using Coolzo.Domain.Entities;
using Coolzo.Domain.Enums;
using Coolzo.Shared.Constants;
using Coolzo.Shared.Exceptions;
using Coolzo.Shared.Models;
using MediatR;

namespace Coolzo.Application.Features.Billing.Commands.RecordPayment;

public sealed class RecordPaymentCommandHandler : IRequestHandler<RecordPaymentCommand, PaymentTransactionResponse>
{
    private readonly IAuditLogRepository _auditLogRepository;
    private readonly BillingAccessService _billingAccessService;
    private readonly IAdminConfigurationRepository _adminConfigurationRepository;
    private readonly IBillingCalculationService _billingCalculationService;
    private readonly IBillingRepository _billingRepository;
    private readonly ICurrentDateTime _currentDateTime;
    private readonly ICurrentUserContext _currentUserContext;
    private readonly IGapPhaseARepository _gapPhaseARepository;
    private readonly IAppLogger<RecordPaymentCommandHandler> _logger;
    private readonly GapPhaseAValidationService _validationService;
    private readonly IReceiptNumberGenerator _receiptNumberGenerator;
    private readonly IUnitOfWork _unitOfWork;

    public RecordPaymentCommandHandler(
        IBillingRepository billingRepository,
        IBillingCalculationService billingCalculationService,
        IReceiptNumberGenerator receiptNumberGenerator,
        BillingAccessService billingAccessService,
        IAdminConfigurationRepository adminConfigurationRepository,
        IGapPhaseARepository gapPhaseARepository,
        GapPhaseAValidationService validationService,
        IAuditLogRepository auditLogRepository,
        IUnitOfWork unitOfWork,
        ICurrentDateTime currentDateTime,
        ICurrentUserContext currentUserContext,
        IAppLogger<RecordPaymentCommandHandler> logger)
    {
        _billingRepository = billingRepository;
        _billingCalculationService = billingCalculationService;
        _receiptNumberGenerator = receiptNumberGenerator;
        _billingAccessService = billingAccessService;
        _adminConfigurationRepository = adminConfigurationRepository;
        _gapPhaseARepository = gapPhaseARepository;
        _validationService = validationService;
        _auditLogRepository = auditLogRepository;
        _unitOfWork = unitOfWork;
        _currentDateTime = currentDateTime;
        _currentUserContext = currentUserContext;
        _logger = logger;
    }

    public async Task<PaymentTransactionResponse> Handle(RecordPaymentCommand request, CancellationToken cancellationToken)
    {
        if (!string.IsNullOrWhiteSpace(request.IdempotencyKey))
        {
            var existingPayment = await _billingRepository.GetPaymentByIdempotencyKeyAsync(request.IdempotencyKey.Trim(), cancellationToken);

            if (existingPayment is not null)
            {
                return BillingResponseMapper.ToPaymentTransactionResponse(existingPayment);
            }
        }

        var invoiceHeader = await _billingRepository.GetInvoiceByIdForUpdateAsync(request.InvoiceId, cancellationToken)
            ?? throw new AppException(ErrorCodes.NotFound, "The requested invoice could not be found.", 404);

        if (!_billingAccessService.HasPaymentCollectAccess())
        {
            await _billingAccessService.EnsureCustomerOwnershipAsync(invoiceHeader.CustomerId, cancellationToken);
        }

        var now = _currentDateTime.UtcNow;
        var actor = ResolveActor();
        PaymentWebhookAttempt? webhookAttempt = null;

        if (request.IsWebhookEvent)
        {
            webhookAttempt = new PaymentWebhookAttempt
            {
                InvoiceHeaderId = invoiceHeader.InvoiceHeaderId,
                IdempotencyKey = request.IdempotencyKey?.Trim() ?? string.Empty,
                GatewayTransactionId = request.GatewayTransactionId?.Trim() ?? string.Empty,
                WebhookReference = request.WebhookReference?.Trim() ?? string.Empty,
                SignatureHash = request.Signature?.Trim() ?? string.Empty,
                PaidAmount = request.PaidAmount,
                PayloadSnapshot = BuildWebhookPayloadSnapshot(request),
                AttemptStatus = PaymentWebhookAttemptStatus.Pending,
                RetryCount = 0,
                LastAttemptDateUtc = now,
                CreatedBy = actor,
                DateCreated = now,
                IPAddress = _currentUserContext.IPAddress
            };

            await _gapPhaseARepository.AddPaymentWebhookAttemptAsync(webhookAttempt, cancellationToken);
        }

        try
        {
            if (!string.IsNullOrWhiteSpace(request.ReferenceNumber) &&
                await _billingRepository.PaymentReferenceExistsAsync(invoiceHeader.InvoiceHeaderId, request.ReferenceNumber.Trim(), cancellationToken))
            {
                throw new AppException(ErrorCodes.DuplicatePaymentDetected, "The payment reference has already been processed.", 409);
            }

            if (!string.IsNullOrWhiteSpace(request.GatewayTransactionId) &&
                await _billingRepository.GatewayTransactionExistsAsync(request.GatewayTransactionId.Trim(), cancellationToken))
            {
                throw new AppException(ErrorCodes.DuplicatePaymentDetected, "The gateway transaction has already been processed.", 409);
            }

            if (request.ExpectedInvoiceAmount.HasValue)
            {
                _validationService.ValidatePaymentAmount(request.ExpectedInvoiceAmount.Value, invoiceHeader.GrandTotalAmount);
            }

            if (request.IsWebhookEvent)
            {
                var secret = await ResolveWebhookSecretAsync(cancellationToken);
                _validationService.ValidateWebhookSignature(
                    invoiceHeader.InvoiceHeaderId,
                    request.PaidAmount,
                    request.ReferenceNumber?.Trim() ?? request.WebhookReference?.Trim() ?? string.Empty,
                    request.Signature,
                    secret);
            }

            if (request.PaidAmount > invoiceHeader.BalanceAmount)
            {
                throw new AppException(ErrorCodes.PaymentAmountExceedsBalance, "The payment amount exceeds the outstanding balance.", 409);
            }

            var nextPaidAmount = invoiceHeader.PaidAmount + request.PaidAmount;
            var nextStatus = _billingCalculationService.ResolvePaymentStatus(invoiceHeader.GrandTotalAmount, nextPaidAmount);
            var nextBalanceAmount = _billingCalculationService.CalculateBalanceAmount(invoiceHeader.GrandTotalAmount, nextPaidAmount);
            var paymentTransaction = new PaymentTransaction
            {
                InvoiceHeader = invoiceHeader,
                PaymentMethod = Enum.Parse<PaymentMethod>(request.PaymentMethod, true),
                ReferenceNumber = request.ReferenceNumber?.Trim() ?? string.Empty,
                IdempotencyKey = request.IdempotencyKey?.Trim() ?? string.Empty,
                GatewayTransactionId = request.GatewayTransactionId?.Trim() ?? string.Empty,
                GatewaySignature = request.Signature?.Trim() ?? string.Empty,
                WebhookReference = request.WebhookReference?.Trim() ?? string.Empty,
                PaidAmount = request.PaidAmount,
                PaymentDateUtc = now,
                TransactionRemarks = request.Remarks?.Trim() ?? string.Empty,
                CreatedBy = actor,
                DateCreated = now,
                IPAddress = _currentUserContext.IPAddress
            };
            var receiptNumber = await GenerateUniqueReceiptNumberAsync(cancellationToken);

            paymentTransaction.PaymentReceipt = new PaymentReceipt
            {
                InvoiceHeader = invoiceHeader,
                ReceiptNumber = receiptNumber,
                ReceiptDateUtc = now,
                ReceivedAmount = request.PaidAmount,
                BalanceAmount = nextBalanceAmount,
                ReceiptRemarks = request.Remarks?.Trim() ?? string.Empty,
                CreatedBy = actor,
                DateCreated = now,
                IPAddress = _currentUserContext.IPAddress
            };

            invoiceHeader.PaidAmount = nextPaidAmount;
            invoiceHeader.BalanceAmount = nextBalanceAmount;
            invoiceHeader.CurrentStatus = nextStatus;
            invoiceHeader.LastPaymentDateUtc = now;
            invoiceHeader.UpdatedBy = actor;
            invoiceHeader.LastUpdated = now;
            invoiceHeader.PaymentTransactions.Add(paymentTransaction);
            invoiceHeader.BillingStatusHistories.Add(new BillingStatusHistory
            {
                QuotationHeaderId = invoiceHeader.QuotationHeaderId,
                EntityType = BillingEntityType.Invoice,
                StatusName = nextStatus.ToString(),
                Remarks = "Invoice payment status updated after payment collection.",
                StatusDateUtc = now,
                CreatedBy = actor,
                DateCreated = now,
                IPAddress = _currentUserContext.IPAddress
            });
            paymentTransaction.BillingStatusHistories.Add(new BillingStatusHistory
            {
                QuotationHeaderId = invoiceHeader.QuotationHeaderId,
                InvoiceHeaderId = invoiceHeader.InvoiceHeaderId,
                EntityType = BillingEntityType.Payment,
                StatusName = "Collected",
                Remarks = BuildPaymentRemarks(paymentTransaction),
                StatusDateUtc = now,
                CreatedBy = actor,
                DateCreated = now,
                IPAddress = _currentUserContext.IPAddress
            });

            if (webhookAttempt is not null)
            {
                webhookAttempt.AttemptStatus = PaymentWebhookAttemptStatus.Processed;
                webhookAttempt.FailureReason = string.Empty;
                webhookAttempt.LastAttemptDateUtc = now;
                webhookAttempt.NextRetryDateUtc = null;
                webhookAttempt.UpdatedBy = actor;
                webhookAttempt.LastUpdated = now;
            }

            await _billingRepository.AddPaymentTransactionAsync(paymentTransaction, cancellationToken);
            await _auditLogRepository.AddAsync(
                new AuditLog
                {
                    UserId = _currentUserContext.UserId,
                    ActionName = "RecordPayment",
                    EntityName = "PaymentTransaction",
                    EntityId = invoiceHeader.InvoiceNumber,
                    TraceId = _currentUserContext.TraceId,
                    StatusName = invoiceHeader.CurrentStatus.ToString(),
                    NewValues = $"{request.PaidAmount:0.00}:{paymentTransaction.PaymentMethod}",
                    CreatedBy = actor,
                    DateCreated = now,
                    IPAddress = _currentUserContext.IPAddress
                },
                cancellationToken);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var savedPayment = (await _billingRepository.GetPaymentTransactionsByInvoiceIdAsync(invoiceHeader.InvoiceHeaderId, cancellationToken))
                .FirstOrDefault(payment => payment.PaymentTransactionId == paymentTransaction.PaymentTransactionId)
                ?? throw new AppException(ErrorCodes.NotFound, "The recorded payment could not be reloaded.", 404);

            _logger.LogInformation(
                "Payment of {PaidAmount} recorded against invoice {InvoiceNumber}.",
                request.PaidAmount,
                invoiceHeader.InvoiceNumber);

            return BillingResponseMapper.ToPaymentTransactionResponse(savedPayment);
        }
        catch (AppException exception) when (webhookAttempt is not null)
        {
            webhookAttempt.AttemptStatus = PaymentWebhookAttemptStatus.RetryPending;
            webhookAttempt.RetryCount += 1;
            webhookAttempt.FailureReason = exception.Message;
            webhookAttempt.LastAttemptDateUtc = now;
            webhookAttempt.NextRetryDateUtc = now.AddMinutes(15);
            webhookAttempt.UpdatedBy = actor;
            webhookAttempt.LastUpdated = now;
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            throw;
        }
    }

    private async Task<string> GenerateUniqueReceiptNumberAsync(CancellationToken cancellationToken)
    {
        while (true)
        {
            var receiptNumber = _receiptNumberGenerator.GenerateNumber();

            if (!await _billingRepository.ReceiptNumberExistsAsync(receiptNumber, cancellationToken))
            {
                return receiptNumber;
            }
        }
    }

    private static string BuildPaymentRemarks(PaymentTransaction paymentTransaction)
    {
        return string.IsNullOrWhiteSpace(paymentTransaction.ReferenceNumber)
            ? $"Payment collected via {paymentTransaction.PaymentMethod}."
            : $"Payment collected via {paymentTransaction.PaymentMethod} with reference {paymentTransaction.ReferenceNumber}.";
    }

    private async Task<string> ResolveWebhookSecretAsync(CancellationToken cancellationToken)
    {
        var configuration = await _adminConfigurationRepository.GetSystemConfigurationByGroupAndKeyAsync(
            "Payments",
            "WebhookSecret",
            null,
            cancellationToken);

        return string.IsNullOrWhiteSpace(configuration?.ConfigurationValue)
            ? "coolzo-gap-a-webhook-secret"
            : configuration.ConfigurationValue;
    }

    private static string BuildWebhookPayloadSnapshot(RecordPaymentCommand request)
    {
        return $"{request.InvoiceId}|{request.PaidAmount:0.00}|{request.PaymentMethod}|{request.ReferenceNumber}|{request.WebhookReference}";
    }

    private string ResolveActor()
    {
        return string.IsNullOrWhiteSpace(_currentUserContext.UserName)
            ? "Payment"
            : _currentUserContext.UserName;
    }
}
