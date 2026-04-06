using Coolzo.Application.Common.Interfaces;
using Coolzo.Application.Common.Models;
using Coolzo.Contracts.Responses.Billing;
using Coolzo.Domain.Entities;
using Coolzo.Domain.Enums;
using Coolzo.Shared.Constants;
using Coolzo.Shared.Exceptions;
using Coolzo.Shared.Models;
using MediatR;

namespace Coolzo.Application.Features.Billing.Commands.GenerateInvoiceFromQuotation;

public sealed class GenerateInvoiceFromQuotationCommandHandler : IRequestHandler<GenerateInvoiceFromQuotationCommand, InvoiceDetailResponse>
{
    private readonly IAuditLogRepository _auditLogRepository;
    private readonly BillingAccessService _billingAccessService;
    private readonly IBillingCalculationService _billingCalculationService;
    private readonly IBillingRepository _billingRepository;
    private readonly ICurrentDateTime _currentDateTime;
    private readonly ICurrentUserContext _currentUserContext;
    private readonly IInvoiceNumberGenerator _invoiceNumberGenerator;
    private readonly IAppLogger<GenerateInvoiceFromQuotationCommandHandler> _logger;
    private readonly IUnitOfWork _unitOfWork;

    public GenerateInvoiceFromQuotationCommandHandler(
        IBillingRepository billingRepository,
        IBillingCalculationService billingCalculationService,
        IInvoiceNumberGenerator invoiceNumberGenerator,
        BillingAccessService billingAccessService,
        IAuditLogRepository auditLogRepository,
        IUnitOfWork unitOfWork,
        ICurrentDateTime currentDateTime,
        ICurrentUserContext currentUserContext,
        IAppLogger<GenerateInvoiceFromQuotationCommandHandler> logger)
    {
        _billingRepository = billingRepository;
        _billingCalculationService = billingCalculationService;
        _invoiceNumberGenerator = invoiceNumberGenerator;
        _billingAccessService = billingAccessService;
        _auditLogRepository = auditLogRepository;
        _unitOfWork = unitOfWork;
        _currentDateTime = currentDateTime;
        _currentUserContext = currentUserContext;
        _logger = logger;
    }

    public async Task<InvoiceDetailResponse> Handle(GenerateInvoiceFromQuotationCommand request, CancellationToken cancellationToken)
    {
        _billingAccessService.EnsureInvoiceCreateAccess();

        var quotationHeader = await _billingRepository.GetQuotationByIdForUpdateAsync(request.QuotationId, cancellationToken)
            ?? throw new AppException(ErrorCodes.NotFound, "The requested quotation could not be found.", 404);

        if (quotationHeader.CurrentStatus != QuotationStatus.Approved)
        {
            throw new AppException(ErrorCodes.QuotationApprovalRequired, "Only approved quotations can be converted into invoices.", 409);
        }

        if (quotationHeader.InvoiceHeader is not null && !quotationHeader.InvoiceHeader.IsDeleted)
        {
            if (!string.IsNullOrWhiteSpace(request.IdempotencyKey))
            {
                var existingInvoice = await _billingRepository.GetInvoiceByIdAsync(quotationHeader.InvoiceHeader.InvoiceHeaderId, cancellationToken)
                    ?? throw new AppException(ErrorCodes.NotFound, "The existing invoice could not be reloaded for idempotent replay.", 404);

                _logger.LogInformation(
                    "Invoice generation idempotency key {IdempotencyKey} reused for quotation {QuotationId}. Returning invoice {InvoiceNumber}.",
                    request.IdempotencyKey,
                    quotationHeader.QuotationHeaderId,
                    existingInvoice.InvoiceNumber);

                return BillingResponseMapper.ToInvoiceDetail(existingInvoice);
            }

            throw new AppException(ErrorCodes.InvoiceAlreadyExists, "An invoice has already been generated for this quotation.", 409);
        }

        var activeLines = quotationHeader.Lines.Where(line => !line.IsDeleted).ToArray();
        var calculation = _billingCalculationService.CalculateInvoice(
            activeLines.Select(line => new BillingLineCalculationInput(line.Quantity, line.UnitPrice)).ToArray(),
            quotationHeader.DiscountAmount,
            quotationHeader.TaxPercentage,
            0.00m);
        var invoiceNumber = await GenerateUniqueInvoiceNumberAsync(cancellationToken);
        var invoiceHeader = new InvoiceHeader
        {
            InvoiceNumber = invoiceNumber,
            QuotationHeader = quotationHeader,
            CustomerId = quotationHeader.CustomerId,
            CurrentStatus = calculation.InvoicePaymentStatus,
            InvoiceDateUtc = _currentDateTime.UtcNow,
            SubTotalAmount = calculation.SubTotalAmount,
            DiscountAmount = calculation.DiscountAmount,
            TaxPercentage = calculation.TaxPercentage,
            TaxAmount = calculation.TaxAmount,
            GrandTotalAmount = calculation.GrandTotalAmount,
            PaidAmount = calculation.PaidAmount,
            BalanceAmount = calculation.BalanceAmount,
            CreatedBy = _currentUserContext.UserName,
            DateCreated = _currentDateTime.UtcNow,
            IPAddress = _currentUserContext.IPAddress
        };

        foreach (var line in activeLines)
        {
            invoiceHeader.Lines.Add(new InvoiceLine
            {
                QuotationLine = line,
                LineType = line.LineType,
                LineDescription = line.LineDescription,
                Quantity = line.Quantity,
                UnitPrice = line.UnitPrice,
                LineAmount = line.LineAmount,
                CreatedBy = _currentUserContext.UserName,
                DateCreated = _currentDateTime.UtcNow,
                IPAddress = _currentUserContext.IPAddress
            });
        }

        quotationHeader.CurrentStatus = QuotationStatus.Invoiced;
        quotationHeader.UpdatedBy = _currentUserContext.UserName;
        quotationHeader.LastUpdated = _currentDateTime.UtcNow;
        quotationHeader.InvoiceHeader = invoiceHeader;
        quotationHeader.BillingStatusHistories.Add(new BillingStatusHistory
        {
            InvoiceHeader = invoiceHeader,
            EntityType = BillingEntityType.Quotation,
            StatusName = QuotationStatus.Invoiced.ToString(),
            Remarks = "Quotation converted to invoice.",
            StatusDateUtc = _currentDateTime.UtcNow,
            CreatedBy = _currentUserContext.UserName,
            DateCreated = _currentDateTime.UtcNow,
            IPAddress = _currentUserContext.IPAddress
        });
        invoiceHeader.BillingStatusHistories.Add(new BillingStatusHistory
        {
            QuotationHeader = quotationHeader,
            EntityType = BillingEntityType.Invoice,
            StatusName = invoiceHeader.CurrentStatus.ToString(),
            Remarks = "Invoice generated from approved quotation.",
            StatusDateUtc = _currentDateTime.UtcNow,
            CreatedBy = _currentUserContext.UserName,
            DateCreated = _currentDateTime.UtcNow,
            IPAddress = _currentUserContext.IPAddress
        });

        await _billingRepository.AddInvoiceAsync(invoiceHeader, cancellationToken);
        await _auditLogRepository.AddAsync(
            new AuditLog
            {
                UserId = _currentUserContext.UserId,
                ActionName = "GenerateInvoiceFromQuotation",
                EntityName = "InvoiceHeader",
                EntityId = invoiceHeader.InvoiceNumber,
                TraceId = _currentUserContext.TraceId,
                StatusName = invoiceHeader.CurrentStatus.ToString(),
                NewValues = invoiceHeader.InvoiceNumber,
                CreatedBy = _currentUserContext.UserName,
                DateCreated = _currentDateTime.UtcNow,
                IPAddress = _currentUserContext.IPAddress
            },
            cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var savedInvoice = await _billingRepository.GetInvoiceByIdAsync(invoiceHeader.InvoiceHeaderId, cancellationToken)
            ?? throw new AppException(ErrorCodes.NotFound, "The generated invoice could not be reloaded.", 404);

        _logger.LogInformation("Invoice {InvoiceNumber} generated from quotation {QuotationNumber}.", savedInvoice.InvoiceNumber, quotationHeader.QuotationNumber);

        return BillingResponseMapper.ToInvoiceDetail(savedInvoice);
    }

    private async Task<string> GenerateUniqueInvoiceNumberAsync(CancellationToken cancellationToken)
    {
        while (true)
        {
            var invoiceNumber = _invoiceNumberGenerator.GenerateNumber();

            if (!await _billingRepository.InvoiceNumberExistsAsync(invoiceNumber, cancellationToken))
            {
                return invoiceNumber;
            }
        }
    }
}
