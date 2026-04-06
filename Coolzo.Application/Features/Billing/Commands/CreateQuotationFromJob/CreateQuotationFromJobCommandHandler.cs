using Coolzo.Application.Common.Interfaces;
using Coolzo.Application.Common.Models;
using Coolzo.Contracts.Responses.Billing;
using Coolzo.Domain.Entities;
using Coolzo.Domain.Enums;
using Coolzo.Shared.Constants;
using Coolzo.Shared.Exceptions;
using Coolzo.Shared.Models;
using MediatR;
using DomainBooking = Coolzo.Domain.Entities.Booking;

namespace Coolzo.Application.Features.Billing.Commands.CreateQuotationFromJob;

public sealed class CreateQuotationFromJobCommandHandler : IRequestHandler<CreateQuotationFromJobCommand, QuotationDetailResponse>
{
    private readonly IAuditLogRepository _auditLogRepository;
    private readonly BillingAccessService _billingAccessService;
    private readonly IBillingCalculationService _billingCalculationService;
    private readonly IBillingRepository _billingRepository;
    private readonly ICurrentDateTime _currentDateTime;
    private readonly ICurrentUserContext _currentUserContext;
    private readonly IAppLogger<CreateQuotationFromJobCommandHandler> _logger;
    private readonly IQuotationNumberGenerator _quotationNumberGenerator;
    private readonly IUnitOfWork _unitOfWork;

    public CreateQuotationFromJobCommandHandler(
        IBillingRepository billingRepository,
        IBillingCalculationService billingCalculationService,
        IQuotationNumberGenerator quotationNumberGenerator,
        BillingAccessService billingAccessService,
        IAuditLogRepository auditLogRepository,
        IUnitOfWork unitOfWork,
        ICurrentDateTime currentDateTime,
        ICurrentUserContext currentUserContext,
        IAppLogger<CreateQuotationFromJobCommandHandler> logger)
    {
        _billingRepository = billingRepository;
        _billingCalculationService = billingCalculationService;
        _quotationNumberGenerator = quotationNumberGenerator;
        _billingAccessService = billingAccessService;
        _auditLogRepository = auditLogRepository;
        _unitOfWork = unitOfWork;
        _currentDateTime = currentDateTime;
        _currentUserContext = currentUserContext;
        _logger = logger;
    }

    public async Task<QuotationDetailResponse> Handle(CreateQuotationFromJobCommand request, CancellationToken cancellationToken)
    {
        var jobCard = await _billingRepository.GetJobCardByIdForUpdateAsync(request.JobCardId, cancellationToken)
            ?? throw new AppException(ErrorCodes.NotFound, "The requested job card could not be found.", 404);

        await _billingAccessService.EnsureTechnicianOwnershipAsync(jobCard, cancellationToken);
        EnsureJobCardIsReady(jobCard);

        var booking = jobCard.ServiceRequest?.Booking
            ?? throw new AppException(ErrorCodes.NotFound, "The booking linked to this job card could not be found.", 404);
        var existingQuotation = await _billingRepository.GetQuotationByJobCardIdAsync(request.JobCardId, cancellationToken);
        var quotationHeader = await CreateOrUpdateQuotationAsync(existingQuotation, jobCard, booking, request, cancellationToken);

        await _auditLogRepository.AddAsync(
            new AuditLog
            {
                UserId = _currentUserContext.UserId,
                ActionName = existingQuotation is null ? "CreateQuotationFromJob" : "ResubmitQuotationFromJob",
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

        var savedQuotation = await _billingRepository.GetQuotationByIdAsync(quotationHeader.QuotationHeaderId, cancellationToken)
            ?? throw new AppException(ErrorCodes.NotFound, "The saved quotation could not be reloaded.", 404);

        _logger.LogInformation(
            "Quotation {QuotationNumber} prepared from job card {JobCardId} by user {UserName}.",
            savedQuotation.QuotationNumber,
            request.JobCardId,
            _currentUserContext.UserName);

        return BillingResponseMapper.ToQuotationDetail(savedQuotation);
    }

    private async Task<QuotationHeader> CreateOrUpdateQuotationAsync(
        QuotationHeader? existingQuotation,
        JobCard jobCard,
        DomainBooking booking,
        CreateQuotationFromJobCommand request,
        CancellationToken cancellationToken)
    {
        var calculation = _billingCalculationService.CalculateInvoice(
            request.Lines.Select(line => new BillingLineCalculationInput(line.Quantity, line.UnitPrice)).ToArray(),
            request.DiscountAmount,
            request.TaxPercentage,
            0.00m);

        var quotationHeader = existingQuotation is null
            ? await CreateNewQuotationAsync(jobCard, booking, calculation, request.Remarks, cancellationToken)
            : await ReuseRejectedQuotationAsync(existingQuotation, calculation, request.Remarks, cancellationToken);

        MarkExistingLinesDeleted(quotationHeader);

        foreach (var line in request.Lines)
        {
            quotationHeader.Lines.Add(new QuotationLine
            {
                LineType = Enum.Parse<QuotationLineType>(line.LineType, true),
                LineDescription = line.LineDescription.Trim(),
                Quantity = line.Quantity,
                UnitPrice = line.UnitPrice,
                LineAmount = _billingCalculationService.CalculateLineAmount(line.Quantity, line.UnitPrice),
                CreatedBy = _currentUserContext.UserName,
                DateCreated = _currentDateTime.UtcNow,
                IPAddress = _currentUserContext.IPAddress
            });
        }

        quotationHeader.BillingStatusHistories.Add(new BillingStatusHistory
        {
            EntityType = BillingEntityType.Quotation,
            StatusName = quotationHeader.CurrentStatus.ToString(),
            Remarks = string.IsNullOrWhiteSpace(request.Remarks)
                ? "Quotation submitted for customer approval."
                : request.Remarks.Trim(),
            StatusDateUtc = _currentDateTime.UtcNow,
            CreatedBy = _currentUserContext.UserName,
            DateCreated = _currentDateTime.UtcNow,
            IPAddress = _currentUserContext.IPAddress
        });

        return quotationHeader;
    }

    private async Task<QuotationHeader> CreateNewQuotationAsync(
        JobCard jobCard,
        DomainBooking booking,
        BillingCalculationResult calculation,
        string? remarks,
        CancellationToken cancellationToken)
    {
        var quotationNumber = await GenerateUniqueQuotationNumberAsync(cancellationToken);
        var quotationHeader = new QuotationHeader
        {
            QuotationNumber = quotationNumber,
            JobCard = jobCard,
            CustomerId = booking.CustomerId,
            CurrentStatus = QuotationStatus.PendingCustomerApproval,
            QuotationDateUtc = _currentDateTime.UtcNow,
            SubTotalAmount = calculation.SubTotalAmount,
            DiscountAmount = calculation.DiscountAmount,
            TaxPercentage = calculation.TaxPercentage,
            TaxAmount = calculation.TaxAmount,
            GrandTotalAmount = calculation.GrandTotalAmount,
            Comments = string.IsNullOrWhiteSpace(remarks) ? null : remarks.Trim(),
            CreatedBy = _currentUserContext.UserName,
            DateCreated = _currentDateTime.UtcNow,
            IPAddress = _currentUserContext.IPAddress
        };

        await _billingRepository.AddQuotationAsync(quotationHeader, cancellationToken);
        return quotationHeader;
    }

    private async Task<QuotationHeader> ReuseRejectedQuotationAsync(
        QuotationHeader existingQuotation,
        BillingCalculationResult calculation,
        string? remarks,
        CancellationToken cancellationToken)
    {
        if (existingQuotation.CurrentStatus is QuotationStatus.PendingCustomerApproval or QuotationStatus.Approved or QuotationStatus.Invoiced)
        {
            throw new AppException(ErrorCodes.QuotationAlreadyExists, "A quotation is already active for this job card.", 409);
        }

        var quotationHeader = await _billingRepository.GetQuotationByIdForUpdateAsync(existingQuotation.QuotationHeaderId, cancellationToken)
            ?? throw new AppException(ErrorCodes.NotFound, "The rejected quotation could not be loaded for resubmission.", 404);

        quotationHeader.CurrentStatus = QuotationStatus.PendingCustomerApproval;
        quotationHeader.QuotationDateUtc = _currentDateTime.UtcNow;
        quotationHeader.SubTotalAmount = calculation.SubTotalAmount;
        quotationHeader.DiscountAmount = calculation.DiscountAmount;
        quotationHeader.TaxPercentage = calculation.TaxPercentage;
        quotationHeader.TaxAmount = calculation.TaxAmount;
        quotationHeader.GrandTotalAmount = calculation.GrandTotalAmount;
        quotationHeader.CustomerDecisionRemarks = string.Empty;
        quotationHeader.ApprovedDateUtc = null;
        quotationHeader.RejectedDateUtc = null;
        quotationHeader.Comments = string.IsNullOrWhiteSpace(remarks) ? quotationHeader.Comments : remarks.Trim();
        quotationHeader.UpdatedBy = _currentUserContext.UserName;
        quotationHeader.LastUpdated = _currentDateTime.UtcNow;

        return quotationHeader;
    }

    private void MarkExistingLinesDeleted(QuotationHeader quotationHeader)
    {
        foreach (var line in quotationHeader.Lines.Where(line => !line.IsDeleted))
        {
            line.IsDeleted = true;
            line.DeletedBy = _currentUserContext.UserName;
            line.DateDeleted = _currentDateTime.UtcNow;
        }
    }

    private async Task<string> GenerateUniqueQuotationNumberAsync(CancellationToken cancellationToken)
    {
        while (true)
        {
            var quotationNumber = _quotationNumberGenerator.GenerateNumber();

            if (!await _billingRepository.QuotationNumberExistsAsync(quotationNumber, cancellationToken))
            {
                return quotationNumber;
            }
        }
    }

    private static void EnsureJobCardIsReady(JobCard jobCard)
    {
        var currentStatus = jobCard.ServiceRequest?.CurrentStatus ?? ServiceRequestStatus.New;

        if (currentStatus is ServiceRequestStatus.Assigned or ServiceRequestStatus.EnRoute or ServiceRequestStatus.Reached)
        {
            throw new AppException(
                ErrorCodes.InvalidStatusTransition,
                "Quotation preparation is allowed only after technician work has started.",
                409);
        }
    }
}
