using Coolzo.Application.Common.Interfaces;
using Coolzo.Contracts.Responses.Billing;
using Coolzo.Domain.Entities;
using Coolzo.Domain.Enums;
using Coolzo.Shared.Constants;
using Coolzo.Shared.Exceptions;
using Coolzo.Shared.Models;
using MediatR;

namespace Coolzo.Application.Features.Billing.Commands.RejectQuotation;

public sealed class RejectQuotationCommandHandler : IRequestHandler<RejectQuotationCommand, QuotationDetailResponse>
{
    private readonly IAuditLogRepository _auditLogRepository;
    private readonly BillingAccessService _billingAccessService;
    private readonly IBillingRepository _billingRepository;
    private readonly ICurrentDateTime _currentDateTime;
    private readonly ICurrentUserContext _currentUserContext;
    private readonly IAppLogger<RejectQuotationCommandHandler> _logger;
    private readonly IUnitOfWork _unitOfWork;

    public RejectQuotationCommandHandler(
        IBillingRepository billingRepository,
        BillingAccessService billingAccessService,
        IAuditLogRepository auditLogRepository,
        IUnitOfWork unitOfWork,
        ICurrentDateTime currentDateTime,
        ICurrentUserContext currentUserContext,
        IAppLogger<RejectQuotationCommandHandler> logger)
    {
        _billingRepository = billingRepository;
        _billingAccessService = billingAccessService;
        _auditLogRepository = auditLogRepository;
        _unitOfWork = unitOfWork;
        _currentDateTime = currentDateTime;
        _currentUserContext = currentUserContext;
        _logger = logger;
    }

    public async Task<QuotationDetailResponse> Handle(RejectQuotationCommand request, CancellationToken cancellationToken)
    {
        var quotationHeader = await _billingRepository.GetQuotationByIdForUpdateAsync(request.QuotationId, cancellationToken)
            ?? throw new AppException(ErrorCodes.NotFound, "The requested quotation could not be found.", 404);

        await _billingAccessService.EnsureCustomerOwnershipAsync(quotationHeader.CustomerId, cancellationToken);

        if (quotationHeader.CurrentStatus != QuotationStatus.PendingCustomerApproval)
        {
            throw new AppException(ErrorCodes.InvalidStatusTransition, "This quotation is not awaiting customer approval.", 409);
        }

        quotationHeader.CurrentStatus = QuotationStatus.Rejected;
        quotationHeader.CustomerDecisionRemarks = request.Remarks?.Trim() ?? string.Empty;
        quotationHeader.ApprovedDateUtc = null;
        quotationHeader.RejectedDateUtc = _currentDateTime.UtcNow;
        quotationHeader.UpdatedBy = _currentUserContext.UserName;
        quotationHeader.LastUpdated = _currentDateTime.UtcNow;
        quotationHeader.BillingStatusHistories.Add(BuildHistory(quotationHeader, request.Remarks));

        await _auditLogRepository.AddAsync(
            new AuditLog
            {
                UserId = _currentUserContext.UserId,
                ActionName = "RejectQuotation",
                EntityName = "QuotationHeader",
                EntityId = quotationHeader.QuotationNumber,
                TraceId = _currentUserContext.TraceId,
                StatusName = quotationHeader.CurrentStatus.ToString(),
                NewValues = quotationHeader.QuotationNumber,
                CreatedBy = _currentUserContext.UserName,
                DateCreated = _currentDateTime.UtcNow,
                IPAddress = _currentUserContext.IPAddress
            },
            cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var savedQuotation = await _billingRepository.GetQuotationByIdAsync(request.QuotationId, cancellationToken)
            ?? throw new AppException(ErrorCodes.NotFound, "The rejected quotation could not be reloaded.", 404);

        _logger.LogInformation("Quotation {QuotationNumber} rejected by user {UserName}.", savedQuotation.QuotationNumber, _currentUserContext.UserName);

        return BillingResponseMapper.ToQuotationDetail(savedQuotation);
    }

    private BillingStatusHistory BuildHistory(QuotationHeader quotationHeader, string? remarks)
    {
        return new BillingStatusHistory
        {
            QuotationHeaderId = quotationHeader.QuotationHeaderId,
            EntityType = BillingEntityType.Quotation,
            StatusName = QuotationStatus.Rejected.ToString(),
            Remarks = remarks?.Trim() ?? "Quotation rejected by customer.",
            StatusDateUtc = _currentDateTime.UtcNow,
            CreatedBy = _currentUserContext.UserName,
            DateCreated = _currentDateTime.UtcNow,
            IPAddress = _currentUserContext.IPAddress
        };
    }
}
