using Coolzo.Application.Common.Interfaces;
using Coolzo.Application.Common.Services;
using Coolzo.Contracts.Responses.GapPhaseA;
using Coolzo.Domain.Entities;
using Coolzo.Domain.Enums;
using Coolzo.Shared.Constants;
using Coolzo.Shared.Exceptions;
using Coolzo.Shared.Models;
using FluentValidation;
using MediatR;

namespace Coolzo.Application.Features.GapPhaseA.Campaign;

public sealed record CreateCampaignCommand(
    string CampaignName,
    long ServiceId,
    long ZoneId,
    long SlotAvailabilityId,
    int PlannedBookingCount,
    DateTime StartDateUtc,
    DateTime EndDateUtc,
    string? Notes) : IRequest<CampaignResponse>;

public sealed class CreateCampaignCommandValidator : AbstractValidator<CreateCampaignCommand>
{
    public CreateCampaignCommandValidator()
    {
        RuleFor(request => request.CampaignName).NotEmpty().MaximumLength(160);
        RuleFor(request => request.ServiceId).GreaterThan(0);
        RuleFor(request => request.ZoneId).GreaterThan(0);
        RuleFor(request => request.SlotAvailabilityId).GreaterThan(0);
        RuleFor(request => request.PlannedBookingCount).GreaterThan(0);
        RuleFor(request => request.EndDateUtc).GreaterThanOrEqualTo(request => request.StartDateUtc);
        RuleFor(request => request.Notes).MaximumLength(512);
    }
}

public sealed class CreateCampaignCommandHandler : IRequestHandler<CreateCampaignCommand, CampaignResponse>
{
    private readonly IAuditLogRepository _auditLogRepository;
    private readonly IBookingLookupRepository _bookingLookupRepository;
    private readonly ICurrentDateTime _currentDateTime;
    private readonly ICurrentUserContext _currentUserContext;
    private readonly GapPhaseAFeatureFlagService _featureFlagService;
    private readonly IGapPhaseAReferenceGenerator _referenceGenerator;
    private readonly IGapPhaseARepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateCampaignCommandHandler(
        IGapPhaseARepository repository,
        IGapPhaseAReferenceGenerator referenceGenerator,
        IBookingLookupRepository bookingLookupRepository,
        GapPhaseAFeatureFlagService featureFlagService,
        IAuditLogRepository auditLogRepository,
        IUnitOfWork unitOfWork,
        ICurrentDateTime currentDateTime,
        ICurrentUserContext currentUserContext)
    {
        _repository = repository;
        _referenceGenerator = referenceGenerator;
        _bookingLookupRepository = bookingLookupRepository;
        _featureFlagService = featureFlagService;
        _auditLogRepository = auditLogRepository;
        _unitOfWork = unitOfWork;
        _currentDateTime = currentDateTime;
        _currentUserContext = currentUserContext;
    }

    public async Task<CampaignResponse> Handle(CreateCampaignCommand request, CancellationToken cancellationToken)
    {
        await _featureFlagService.EnsureEnabledAsync("gap.phaseA.campaign.enabled", cancellationToken);

        var service = await _bookingLookupRepository.GetServiceByIdAsync(request.ServiceId, cancellationToken)
            ?? throw new AppException(ErrorCodes.InvalidMasterSelection, "The selected service is invalid.", 400);
        _ = service;
        var slotAvailability = await _bookingLookupRepository.GetSlotAvailabilityByIdAsync(request.SlotAvailabilityId, cancellationToken)
            ?? throw new AppException(ErrorCodes.SlotUnavailable, "The selected slot could not be found.", 404);

        if (slotAvailability.ZoneId != request.ZoneId ||
            slotAvailability.IsBlocked ||
            slotAvailability.ReservedCapacity + request.PlannedBookingCount > slotAvailability.AvailableCapacity)
        {
            throw new AppException(ErrorCodes.CampaignCapacityExceeded, "The selected slot does not have enough remaining capacity for the campaign.", 409);
        }

        var campaignCode = await GenerateUniqueCampaignCodeAsync(cancellationToken);
        slotAvailability.ReservedCapacity += request.PlannedBookingCount;
        var now = _currentDateTime.UtcNow;
        var actor = ResolveActor();
        var campaign = new Domain.Entities.Campaign
        {
            CampaignCode = campaignCode,
            CampaignName = request.CampaignName.Trim(),
            ServiceId = request.ServiceId,
            ZoneId = request.ZoneId,
            SlotAvailabilityId = request.SlotAvailabilityId,
            CampaignStatus = CampaignStatus.Active,
            PlannedBookingCount = request.PlannedBookingCount,
            AllocatedBookingCount = request.PlannedBookingCount,
            StartDateUtc = request.StartDateUtc,
            EndDateUtc = request.EndDateUtc,
            Notes = request.Notes?.Trim() ?? string.Empty,
            CreatedBy = actor,
            DateCreated = now,
            IPAddress = _currentUserContext.IPAddress
        };

        await _repository.AddCampaignAsync(campaign, cancellationToken);
        await _auditLogRepository.AddAsync(
            new AuditLog
            {
                UserId = _currentUserContext.UserId,
                ActionName = "CreateCampaign",
                EntityName = nameof(Domain.Entities.Campaign),
                EntityId = campaign.CampaignCode,
                TraceId = _currentUserContext.TraceId,
                StatusName = "Success",
                NewValues = request.PlannedBookingCount.ToString(),
                CreatedBy = actor,
                DateCreated = now,
                IPAddress = _currentUserContext.IPAddress
            },
            cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new CampaignResponse(
            campaign.CampaignId,
            campaign.CampaignCode,
            campaign.CampaignName,
            campaign.CampaignStatus.ToString(),
            campaign.PlannedBookingCount,
            campaign.AllocatedBookingCount,
            campaign.SlotAvailabilityId);
    }

    private async Task<string> GenerateUniqueCampaignCodeAsync(CancellationToken cancellationToken)
    {
        while (true)
        {
            var campaignCode = _referenceGenerator.GenerateCampaignCode();

            if (!await _repository.CampaignCodeExistsAsync(campaignCode, cancellationToken))
            {
                return campaignCode;
            }
        }
    }

    private string ResolveActor()
    {
        return string.IsNullOrWhiteSpace(_currentUserContext.UserName) ? "Campaign" : _currentUserContext.UserName;
    }
}
