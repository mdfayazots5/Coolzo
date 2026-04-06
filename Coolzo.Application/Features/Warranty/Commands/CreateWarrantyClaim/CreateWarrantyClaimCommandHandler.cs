using Coolzo.Application.Common.Interfaces;
using Coolzo.Application.Features.Amc;
using Coolzo.Contracts.Responses.Warranty;
using Coolzo.Domain.Entities;
using Coolzo.Domain.Enums;
using Coolzo.Shared.Constants;
using Coolzo.Shared.Exceptions;
using Coolzo.Shared.Models;
using MediatR;

namespace Coolzo.Application.Features.Warranty.Commands.CreateWarrantyClaim;

public sealed class CreateWarrantyClaimCommandHandler : IRequestHandler<CreateWarrantyClaimCommand, WarrantyClaimResponse>
{
    private readonly IAmcRepository _amcRepository;
    private readonly IAuditLogRepository _auditLogRepository;
    private readonly ICurrentDateTime _currentDateTime;
    private readonly ICurrentUserContext _currentUserContext;
    private readonly IAppLogger<CreateWarrantyClaimCommandHandler> _logger;
    private readonly ServiceLifecycleAccessService _serviceLifecycleAccessService;
    private readonly IUnitOfWork _unitOfWork;

    public CreateWarrantyClaimCommandHandler(
        IAmcRepository amcRepository,
        IAuditLogRepository auditLogRepository,
        IUnitOfWork unitOfWork,
        ICurrentDateTime currentDateTime,
        ICurrentUserContext currentUserContext,
        ServiceLifecycleAccessService serviceLifecycleAccessService,
        IAppLogger<CreateWarrantyClaimCommandHandler> logger)
    {
        _amcRepository = amcRepository;
        _auditLogRepository = auditLogRepository;
        _unitOfWork = unitOfWork;
        _currentDateTime = currentDateTime;
        _currentUserContext = currentUserContext;
        _serviceLifecycleAccessService = serviceLifecycleAccessService;
        _logger = logger;
    }

    public async Task<WarrantyClaimResponse> Handle(CreateWarrantyClaimCommand request, CancellationToken cancellationToken)
    {
        var invoiceHeader = await _amcRepository.GetInvoiceByIdAsync(request.InvoiceId, cancellationToken)
            ?? throw new AppException(ErrorCodes.NotFound, "The requested invoice could not be found.", 404);

        await _serviceLifecycleAccessService.EnsureWarrantyClaimCreateAccessAsync(invoiceHeader, cancellationToken);

        var primaryLine = WarrantyEligibilityHelper.GetPrimaryBookingLine(invoiceHeader)
            ?? throw new AppException(ErrorCodes.NotFound, "The invoice is not linked to a service line eligible for warranty evaluation.", 404);
        var warrantyRule = await _amcRepository.GetMatchingWarrantyRuleAsync(
            primaryLine.ServiceId,
            primaryLine.AcTypeId,
            primaryLine.BrandId,
            cancellationToken);

        if (warrantyRule is null)
        {
            throw new AppException(ErrorCodes.WarrantyNotEligible, "No active warranty rule exists for the billed service.", 409);
        }

        var (coverageStartDateUtc, coverageEndDateUtc, isEligible) = WarrantyEligibilityHelper.EvaluateCoverage(
            invoiceHeader,
            warrantyRule,
            _currentDateTime.UtcNow);

        if (!isEligible)
        {
            throw new AppException(ErrorCodes.WarrantyNotEligible, "The warranty period for this invoice has already expired.", 409);
        }

        if (await _amcRepository.HasOpenWarrantyClaimAsync(invoiceHeader.InvoiceHeaderId, cancellationToken))
        {
            throw new AppException(ErrorCodes.Conflict, "An active warranty claim already exists for this invoice.", 409);
        }

        var warrantyClaim = new WarrantyClaim
        {
            InvoiceHeaderId = invoiceHeader.InvoiceHeaderId,
            CustomerId = invoiceHeader.CustomerId,
            JobCardId = invoiceHeader.QuotationHeader?.JobCardId
                ?? throw new AppException(ErrorCodes.NotFound, "The invoice is not linked to a job card.", 404),
            WarrantyRuleId = warrantyRule.WarrantyRuleId,
            CurrentStatus = WarrantyClaimStatus.Submitted,
            ClaimDateUtc = _currentDateTime.UtcNow,
            CoverageStartDateUtc = coverageStartDateUtc,
            CoverageEndDateUtc = coverageEndDateUtc,
            IsEligible = true,
            ClaimRemarks = request.ClaimRemarks?.Trim() ?? string.Empty,
            CreatedBy = _currentUserContext.UserName,
            DateCreated = _currentDateTime.UtcNow,
            IPAddress = _currentUserContext.IPAddress
        };

        await _amcRepository.AddWarrantyClaimAsync(warrantyClaim, cancellationToken);
        await AddAuditLogAsync(invoiceHeader.InvoiceNumber, warrantyClaim.CurrentStatus.ToString(), cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var savedClaim = await _amcRepository.GetWarrantyClaimByIdAsync(warrantyClaim.WarrantyClaimId, cancellationToken)
            ?? throw new AppException(ErrorCodes.NotFound, "The saved warranty claim could not be reloaded.", 404);

        _logger.LogInformation(
            "Warranty claim {WarrantyClaimId} created for invoice {InvoiceNumber}.",
            savedClaim.WarrantyClaimId,
            invoiceHeader.InvoiceNumber);

        return WarrantyResponseMapper.ToClaim(savedClaim);
    }

    private Task AddAuditLogAsync(string invoiceNumber, string statusName, CancellationToken cancellationToken)
    {
        return _auditLogRepository.AddAsync(
            new AuditLog
            {
                UserId = _currentUserContext.UserId,
                ActionName = "CreateWarrantyClaim",
                EntityName = "WarrantyClaim",
                EntityId = invoiceNumber,
                TraceId = _currentUserContext.TraceId,
                StatusName = statusName,
                NewValues = invoiceNumber,
                CreatedBy = _currentUserContext.UserName,
                DateCreated = _currentDateTime.UtcNow,
                IPAddress = _currentUserContext.IPAddress
            },
            cancellationToken);
    }
}
