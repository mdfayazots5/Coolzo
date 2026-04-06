using Coolzo.Application.Common.Interfaces;
using Coolzo.Contracts.Responses.Billing;
using Coolzo.Domain.Entities;
using Coolzo.Domain.Enums;
using Coolzo.Shared.Constants;
using Coolzo.Shared.Exceptions;
using Coolzo.Shared.Models;
using MediatR;

namespace Coolzo.Application.Features.Billing.Commands.ApproveQuotation;

public sealed class ApproveQuotationCommandHandler : IRequestHandler<ApproveQuotationCommand, QuotationDetailResponse>
{
    private readonly IAuditLogRepository _auditLogRepository;
    private readonly BillingAccessService _billingAccessService;
    private readonly IBillingRepository _billingRepository;
    private readonly ICurrentDateTime _currentDateTime;
    private readonly ICurrentUserContext _currentUserContext;
    private readonly IAppLogger<ApproveQuotationCommandHandler> _logger;
    private readonly IUnitOfWork _unitOfWork;

    public ApproveQuotationCommandHandler(
        IBillingRepository billingRepository,
        BillingAccessService billingAccessService,
        IAuditLogRepository auditLogRepository,
        IUnitOfWork unitOfWork,
        ICurrentDateTime currentDateTime,
        ICurrentUserContext currentUserContext,
        IAppLogger<ApproveQuotationCommandHandler> logger)
    {
        _billingRepository = billingRepository;
        _billingAccessService = billingAccessService;
        _auditLogRepository = auditLogRepository;
        _unitOfWork = unitOfWork;
        _currentDateTime = currentDateTime;
        _currentUserContext = currentUserContext;
        _logger = logger;
    }

    public async Task<QuotationDetailResponse> Handle(ApproveQuotationCommand request, CancellationToken cancellationToken)
    {
        var quotationHeader = await _billingRepository.GetQuotationByIdForUpdateAsync(request.QuotationId, cancellationToken)
            ?? throw new AppException(ErrorCodes.NotFound, "The requested quotation could not be found.", 404);

        await _billingAccessService.EnsureCustomerOwnershipAsync(quotationHeader.CustomerId, cancellationToken);

        if (quotationHeader.CurrentStatus != QuotationStatus.PendingCustomerApproval)
        {
            throw new AppException(ErrorCodes.InvalidStatusTransition, "This quotation is not awaiting customer approval.", 409);
        }

        quotationHeader.CurrentStatus = QuotationStatus.Approved;
        quotationHeader.CustomerDecisionRemarks = request.Remarks?.Trim() ?? string.Empty;
        quotationHeader.ApprovedDateUtc = _currentDateTime.UtcNow;
        quotationHeader.RejectedDateUtc = null;
        quotationHeader.UpdatedBy = _currentUserContext.UserName;
        quotationHeader.LastUpdated = _currentDateTime.UtcNow;
        quotationHeader.BillingStatusHistories.Add(BuildHistory(quotationHeader, request.Remarks));

        await _auditLogRepository.AddAsync(
            new AuditLog
            {
                UserId = _currentUserContext.UserId,
                ActionName = "ApproveQuotation",
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
            ?? throw new AppException(ErrorCodes.NotFound, "The approved quotation could not be reloaded.", 404);

        _logger.LogInformation("Quotation {QuotationNumber} approved by user {UserName}.", savedQuotation.QuotationNumber, _currentUserContext.UserName);

        return BillingResponseMapper.ToQuotationDetail(savedQuotation);
    }

    private BillingStatusHistory BuildHistory(QuotationHeader quotationHeader, string? remarks)
    {
        return new BillingStatusHistory
        {
            QuotationHeaderId = quotationHeader.QuotationHeaderId,
            EntityType = BillingEntityType.Quotation,
            StatusName = QuotationStatus.Approved.ToString(),
            Remarks = string.IsNullOrWhiteSpace(remarks)
                ? "Quotation approved by customer."
                : remarks.Trim(),
            StatusDateUtc = _currentDateTime.UtcNow,
            CreatedBy = _currentUserContext.UserName,
            DateCreated = _currentDateTime.UtcNow,
            IPAddress = _currentUserContext.IPAddress
        };
    }
}
