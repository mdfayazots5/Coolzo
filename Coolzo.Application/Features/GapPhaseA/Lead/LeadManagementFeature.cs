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
using DomainBooking = Coolzo.Domain.Entities.Booking;
using DomainServiceRequest = Coolzo.Domain.Entities.ServiceRequest;

namespace Coolzo.Application.Features.GapPhaseA.Lead;

public sealed record CreateLeadCommand(
    string CustomerName,
    string MobileNumber,
    string? EmailAddress,
    string SourceChannel,
    string? AddressLine1,
    string? AddressLine2,
    string? CityName,
    string? Pincode,
    long? ServiceId,
    long? AcTypeId,
    long? TonnageId,
    long? BrandId,
    long? SlotAvailabilityId,
    string? InquiryNotes) : IRequest<LeadResponse>;

public sealed class CreateLeadCommandValidator : AbstractValidator<CreateLeadCommand>
{
    public CreateLeadCommandValidator()
    {
        RuleFor(request => request.CustomerName).NotEmpty().MaximumLength(128);
        RuleFor(request => request.MobileNumber).Matches("^[0-9]{8,16}$");
        RuleFor(request => request.EmailAddress).EmailAddress().When(request => !string.IsNullOrWhiteSpace(request.EmailAddress));
        RuleFor(request => request.SourceChannel).NotEmpty().Must(LeadManagementSupport.TryParseLeadSourceChannel).WithMessage("Lead source channel is invalid.");
        RuleFor(request => request.AddressLine1).MaximumLength(256);
        RuleFor(request => request.AddressLine2).MaximumLength(256);
        RuleFor(request => request.CityName).MaximumLength(128);
        RuleFor(request => request.Pincode).MaximumLength(16);
        RuleFor(request => request.InquiryNotes).MaximumLength(512);
    }
}

public sealed class CreateLeadCommandHandler : IRequestHandler<CreateLeadCommand, LeadResponse>
{
    private static readonly TimeSpan DuplicateLeadWindow = TimeSpan.FromDays(7);

    private readonly IAuditLogRepository _auditLogRepository;
    private readonly ICurrentDateTime _currentDateTime;
    private readonly ICurrentUserContext _currentUserContext;
    private readonly GapPhaseAFeatureFlagService _featureFlagService;
    private readonly IGapPhaseAReferenceGenerator _referenceGenerator;
    private readonly IGapPhaseARepository _repository;
    private readonly GapPhaseANotificationService _notificationService;
    private readonly IUnitOfWork _unitOfWork;

    public CreateLeadCommandHandler(
        IGapPhaseARepository repository,
        IGapPhaseAReferenceGenerator referenceGenerator,
        IAuditLogRepository auditLogRepository,
        IUnitOfWork unitOfWork,
        ICurrentDateTime currentDateTime,
        ICurrentUserContext currentUserContext,
        GapPhaseANotificationService notificationService,
        GapPhaseAFeatureFlagService featureFlagService)
    {
        _repository = repository;
        _referenceGenerator = referenceGenerator;
        _auditLogRepository = auditLogRepository;
        _unitOfWork = unitOfWork;
        _currentDateTime = currentDateTime;
        _currentUserContext = currentUserContext;
        _notificationService = notificationService;
        _featureFlagService = featureFlagService;
    }

    public async Task<LeadResponse> Handle(CreateLeadCommand request, CancellationToken cancellationToken)
    {
        await _featureFlagService.EnsureEnabledAsync("gap.phaseA.lead.enabled", cancellationToken);

        var now = _currentDateTime.UtcNow;
        var actor = LeadManagementSupport.ResolveActor(_currentUserContext, "LeadCapture");
        var normalizedMobile = request.MobileNumber.Trim();

        if (await _repository.ActiveLeadExistsByMobileAsync(normalizedMobile, now.Subtract(DuplicateLeadWindow), cancellationToken))
        {
            throw new AppException(
                ErrorCodes.LeadDuplicateDetected,
                "A lead for this mobile number already exists within the active duplicate-detection window.",
                409);
        }

        LeadManagementSupport.TryParseLeadSourceChannel(request.SourceChannel, out var sourceChannel);

        var lead = new Domain.Entities.Lead
        {
            LeadNumber = await GenerateLeadNumberAsync(cancellationToken),
            CustomerName = request.CustomerName.Trim(),
            MobileNumber = normalizedMobile,
            EmailAddress = request.EmailAddress?.Trim() ?? string.Empty,
            SourceChannel = sourceChannel,
            LeadStatus = LeadStatus.New,
            ServiceId = request.ServiceId,
            AcTypeId = request.AcTypeId,
            TonnageId = request.TonnageId,
            BrandId = request.BrandId,
            SlotAvailabilityId = request.SlotAvailabilityId,
            AddressLine1 = request.AddressLine1?.Trim() ?? string.Empty,
            AddressLine2 = request.AddressLine2?.Trim() ?? string.Empty,
            CityName = request.CityName?.Trim() ?? string.Empty,
            Pincode = request.Pincode?.Trim() ?? string.Empty,
            InquiryNotes = request.InquiryNotes?.Trim() ?? string.Empty,
            CreatedBy = actor,
            DateCreated = now,
            IPAddress = _currentUserContext.IPAddress
        };

        lead.StatusHistories.Add(new LeadStatusHistory
        {
            PreviousStatus = LeadStatus.New,
            CurrentStatus = LeadStatus.New,
            Remarks = "Lead captured.",
            ChangedDateUtc = now,
            CreatedBy = actor,
            DateCreated = now,
            IPAddress = _currentUserContext.IPAddress
        });

        await _repository.AddLeadAsync(lead, cancellationToken);
        await _notificationService.RaiseAlertAsync(
            "lead.received",
            "lead.received",
            "Lead",
            nameof(Domain.Entities.Lead),
            lead.LeadNumber,
            SystemAlertSeverity.Info,
            $"Lead {lead.LeadNumber} captured from {LeadManagementSupport.ToDisplayLeadSourceChannel(lead.SourceChannel)}.",
            now.AddMinutes(15),
            1,
            "CustomerSupportExecutive>OperationsExecutive",
            cancellationToken);
        await LeadManagementSupport.WriteLeadAuditAsync(
            _auditLogRepository,
            _currentUserContext,
            "CreateLead",
            lead.LeadNumber,
            LeadManagementSupport.ToDisplayLeadSourceChannel(lead.SourceChannel),
            actor,
            now,
            cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return LeadManagementSupport.MapLead(lead);
    }

    private async Task<string> GenerateLeadNumberAsync(CancellationToken cancellationToken)
    {
        while (true)
        {
            var leadNumber = _referenceGenerator.GenerateLeadNumber();

            if (!await _repository.LeadNumberExistsAsync(leadNumber, cancellationToken))
            {
                return leadNumber;
            }
        }
    }
}

public sealed record ConvertLeadToBookingCommand(
    long LeadId,
    long? ServiceId,
    long? AcTypeId,
    long? TonnageId,
    long? BrandId,
    long? SlotAvailabilityId,
    string? AddressLine1,
    string? AddressLine2,
    string? CityName,
    string? Pincode,
    string? InquiryNotes) : IRequest<LeadResponse>;

public sealed class ConvertLeadToBookingCommandValidator : AbstractValidator<ConvertLeadToBookingCommand>
{
    public ConvertLeadToBookingCommandValidator()
    {
        RuleFor(request => request.LeadId).GreaterThan(0);
        RuleFor(request => request.AddressLine1).MaximumLength(256);
        RuleFor(request => request.AddressLine2).MaximumLength(256);
        RuleFor(request => request.CityName).MaximumLength(128);
        RuleFor(request => request.Pincode).MaximumLength(16);
        RuleFor(request => request.InquiryNotes).MaximumLength(512);
    }
}

public sealed class ConvertLeadToServiceRequestCommandValidator : AbstractValidator<ConvertLeadToServiceRequestCommand>
{
    public ConvertLeadToServiceRequestCommandValidator()
    {
        RuleFor(request => request.LeadId).GreaterThan(0);
        RuleFor(request => request.AddressLine1).MaximumLength(256);
        RuleFor(request => request.AddressLine2).MaximumLength(256);
        RuleFor(request => request.CityName).MaximumLength(128);
        RuleFor(request => request.Pincode).MaximumLength(16);
        RuleFor(request => request.InquiryNotes).MaximumLength(512);
    }
}

public sealed class ConvertLeadToBookingCommandHandler : LeadConversionCommandHandlerBase, IRequestHandler<ConvertLeadToBookingCommand, LeadResponse>
{
    public ConvertLeadToBookingCommandHandler(
        IGapPhaseARepository repository,
        IBookingLookupRepository bookingLookupRepository,
        IBookingRepository bookingRepository,
        IBookingReferenceGenerator bookingReferenceGenerator,
        GapPhaseAValidationService validationService,
        GapPhaseAWorkflowService workflowService,
        GapPhaseAFeatureFlagService featureFlagService,
        IAuditLogRepository auditLogRepository,
        IUnitOfWork unitOfWork,
        ICurrentDateTime currentDateTime,
        ICurrentUserContext currentUserContext)
        : base(
            repository,
            bookingLookupRepository,
            bookingRepository,
            bookingReferenceGenerator,
            validationService,
            workflowService,
            featureFlagService,
            auditLogRepository,
            unitOfWork,
            currentDateTime,
            currentUserContext)
    {
    }

    public async Task<LeadResponse> Handle(ConvertLeadToBookingCommand request, CancellationToken cancellationToken)
    {
        await EnsureLeadFeatureEnabledAsync(cancellationToken);

        var lead = await GetLeadForUpdateAsync(request.LeadId, cancellationToken);

        if (lead.ConvertedBookingId.HasValue)
        {
            throw new AppException(ErrorCodes.LeadBookingAlreadyCreated, "A booking has already been created for this lead.", 409);
        }

        EnsureLeadReadyForInitialConversion(lead);

        var actor = ResolveActor("LeadConversion");
        var now = CurrentUtcNow;
        var details = await ResolveLeadConversionDetailsAsync(lead, request, cancellationToken);
        var booking = await CreateBookingAsync(lead, details, actor, now, cancellationToken);

        ApplyResolvedLeadDetails(lead, details);
        lead.ConvertedBookingId = booking.BookingId;

        await Repository.AddLeadConversionAsync(
            new LeadConversion
            {
                Lead = lead,
                ConversionType = LeadConversionType.Booking,
                BookingId = booking.BookingId,
                ReferenceNumber = booking.BookingReference,
                Remarks = $"Lead converted to booking {booking.BookingReference}.",
                ConvertedDateUtc = now,
                CreatedBy = actor,
                DateCreated = now,
                IPAddress = CurrentUserContext.IPAddress
            },
            cancellationToken);

        await WorkflowService.ChangeLeadStatusAsync(lead, LeadStatus.Converted, $"Lead converted to booking {booking.BookingReference}.", cancellationToken);
        await WriteAuditAsync("ConvertLeadToBooking", lead.LeadNumber, booking.BookingReference, actor, now, cancellationToken);
        await UnitOfWork.SaveChangesAsync(cancellationToken);

        return LeadManagementSupport.MapLead(lead);
    }
}

public sealed record ConvertLeadToServiceRequestCommand(
    long LeadId,
    long? ServiceId,
    long? AcTypeId,
    long? TonnageId,
    long? BrandId,
    long? SlotAvailabilityId,
    string? AddressLine1,
    string? AddressLine2,
    string? CityName,
    string? Pincode,
    string? InquiryNotes) : IRequest<LeadResponse>;

public sealed class ConvertLeadToServiceRequestCommandHandler : LeadConversionCommandHandlerBase, IRequestHandler<ConvertLeadToServiceRequestCommand, LeadResponse>
{
    private readonly IServiceRequestNumberGenerator _serviceRequestNumberGenerator;
    private readonly IServiceRequestRepository _serviceRequestRepository;

    public ConvertLeadToServiceRequestCommandHandler(
        IGapPhaseARepository repository,
        IBookingLookupRepository bookingLookupRepository,
        IBookingRepository bookingRepository,
        IServiceRequestRepository serviceRequestRepository,
        IBookingReferenceGenerator bookingReferenceGenerator,
        IServiceRequestNumberGenerator serviceRequestNumberGenerator,
        GapPhaseAValidationService validationService,
        GapPhaseAWorkflowService workflowService,
        GapPhaseAFeatureFlagService featureFlagService,
        IAuditLogRepository auditLogRepository,
        IUnitOfWork unitOfWork,
        ICurrentDateTime currentDateTime,
        ICurrentUserContext currentUserContext)
        : base(
            repository,
            bookingLookupRepository,
            bookingRepository,
            bookingReferenceGenerator,
            validationService,
            workflowService,
            featureFlagService,
            auditLogRepository,
            unitOfWork,
            currentDateTime,
            currentUserContext)
    {
        _serviceRequestRepository = serviceRequestRepository;
        _serviceRequestNumberGenerator = serviceRequestNumberGenerator;
    }

    public async Task<LeadResponse> Handle(ConvertLeadToServiceRequestCommand request, CancellationToken cancellationToken)
    {
        await EnsureLeadFeatureEnabledAsync(cancellationToken);

        var lead = await GetLeadForUpdateAsync(request.LeadId, cancellationToken);

        if (lead.ConvertedServiceRequestId.HasValue)
        {
            throw new AppException(ErrorCodes.LeadAlreadyConverted, "A service request has already been created for this lead.", 409);
        }

        var actor = ResolveActor("LeadConversion");
        var now = CurrentUtcNow;
        DomainBooking booking;

        if (lead.ConvertedBookingId.HasValue)
        {
            booking = await BookingRepository.GetByIdAsync(lead.ConvertedBookingId.Value, cancellationToken)
                ?? throw new AppException(ErrorCodes.NotFound, "The linked booking could not be found for this lead.", 404);

            if (lead.LeadStatus != LeadStatus.Qualified && lead.LeadStatus != LeadStatus.Converted)
            {
                throw new AppException(ErrorCodes.InvalidStatusTransition, "Only qualified or already-converted leads can progress to service request creation.", 409);
            }
        }
        else
        {
            EnsureLeadReadyForInitialConversion(lead);
            var details = await ResolveLeadConversionDetailsAsync(lead, request, cancellationToken);
            booking = await CreateBookingAsync(lead, details, actor, now, cancellationToken);
            ApplyResolvedLeadDetails(lead, details);
            lead.ConvertedBookingId = booking.BookingId;

            await Repository.AddLeadConversionAsync(
                new LeadConversion
                {
                    Lead = lead,
                    ConversionType = LeadConversionType.Booking,
                    BookingId = booking.BookingId,
                    ReferenceNumber = booking.BookingReference,
                    Remarks = $"Lead converted to booking {booking.BookingReference}.",
                    ConvertedDateUtc = now,
                    CreatedBy = actor,
                    DateCreated = now,
                    IPAddress = CurrentUserContext.IPAddress
                },
                cancellationToken);
        }

        if (booking.ServiceRequest is not null && !booking.ServiceRequest.IsDeleted)
        {
            throw new AppException(ErrorCodes.ServiceRequestAlreadyExists, "A service request already exists for the lead booking.", 409);
        }

        var serviceRequestNumber = await GenerateUniqueServiceRequestNumberAsync(cancellationToken);
        var serviceRequest = new DomainServiceRequest
        {
            Booking = booking,
            ServiceRequestNumber = serviceRequestNumber,
            CurrentStatus = ServiceRequestStatus.New,
            ServiceRequestDateUtc = now,
            CreatedBy = actor,
            DateCreated = now,
            IPAddress = CurrentUserContext.IPAddress
        };

        serviceRequest.StatusHistories.Add(new ServiceRequestStatusHistory
        {
            Status = ServiceRequestStatus.New,
            Remarks = $"Service request created from lead {lead.LeadNumber}.",
            StatusDateUtc = now,
            CreatedBy = actor,
            DateCreated = now,
            IPAddress = CurrentUserContext.IPAddress
        });

        await _serviceRequestRepository.AddAsync(serviceRequest, cancellationToken);
        lead.ConvertedServiceRequestId = serviceRequest.ServiceRequestId;

        await Repository.AddLeadConversionAsync(
            new LeadConversion
            {
                Lead = lead,
                ConversionType = LeadConversionType.ServiceRequest,
                BookingId = booking.BookingId,
                ServiceRequestId = serviceRequest.ServiceRequestId,
                ReferenceNumber = serviceRequestNumber,
                Remarks = $"Lead converted to service request {serviceRequestNumber}.",
                ConvertedDateUtc = now,
                CreatedBy = actor,
                DateCreated = now,
                IPAddress = CurrentUserContext.IPAddress
            },
            cancellationToken);

        if (lead.LeadStatus == LeadStatus.Qualified)
        {
            await WorkflowService.ChangeLeadStatusAsync(lead, LeadStatus.Converted, $"Lead converted to service request {serviceRequestNumber}.", cancellationToken);
        }
        else
        {
            lead.UpdatedBy = actor;
            lead.LastUpdated = now;
            lead.ConvertedDateUtc = now;
        }

        await WriteAuditAsync("ConvertLeadToServiceRequest", lead.LeadNumber, serviceRequestNumber, actor, now, cancellationToken);
        await UnitOfWork.SaveChangesAsync(cancellationToken);

        lead.ConvertedServiceRequestId = serviceRequest.ServiceRequestId;

        return LeadManagementSupport.MapLead(lead);
    }

    private async Task<string> GenerateUniqueServiceRequestNumberAsync(CancellationToken cancellationToken)
    {
        while (true)
        {
            var serviceRequestNumber = _serviceRequestNumberGenerator.GenerateNumber();

            if (!await _serviceRequestRepository.ServiceRequestNumberExistsAsync(serviceRequestNumber, cancellationToken))
            {
                return serviceRequestNumber;
            }
        }
    }
}

public abstract class LeadConversionCommandHandlerBase
{
    private readonly IAuditLogRepository _auditLogRepository;
    private readonly IBookingLookupRepository _bookingLookupRepository;
    private readonly IBookingReferenceGenerator _bookingReferenceGenerator;
    private readonly GapPhaseAFeatureFlagService _featureFlagService;
    private readonly GapPhaseAValidationService _validationService;

    protected LeadConversionCommandHandlerBase(
        IGapPhaseARepository repository,
        IBookingLookupRepository bookingLookupRepository,
        IBookingRepository bookingRepository,
        IBookingReferenceGenerator bookingReferenceGenerator,
        GapPhaseAValidationService validationService,
        GapPhaseAWorkflowService workflowService,
        GapPhaseAFeatureFlagService featureFlagService,
        IAuditLogRepository auditLogRepository,
        IUnitOfWork unitOfWork,
        ICurrentDateTime currentDateTime,
        ICurrentUserContext currentUserContext)
    {
        Repository = repository;
        _bookingLookupRepository = bookingLookupRepository;
        BookingRepository = bookingRepository;
        _bookingReferenceGenerator = bookingReferenceGenerator;
        _validationService = validationService;
        WorkflowService = workflowService;
        _featureFlagService = featureFlagService;
        _auditLogRepository = auditLogRepository;
        UnitOfWork = unitOfWork;
        CurrentDateTime = currentDateTime;
        CurrentUserContext = currentUserContext;
    }

    protected IGapPhaseARepository Repository { get; }

    protected IBookingRepository BookingRepository { get; }

    protected GapPhaseAWorkflowService WorkflowService { get; }

    protected IUnitOfWork UnitOfWork { get; }

    protected ICurrentDateTime CurrentDateTime { get; }

    protected ICurrentUserContext CurrentUserContext { get; }

    protected DateTime CurrentUtcNow => CurrentDateTime.UtcNow;

    protected async Task EnsureLeadFeatureEnabledAsync(CancellationToken cancellationToken)
    {
        await _featureFlagService.EnsureEnabledAsync("gap.phaseA.lead.enabled", cancellationToken);
    }

    protected async Task<Domain.Entities.Lead> GetLeadForUpdateAsync(long leadId, CancellationToken cancellationToken)
    {
        return await Repository.GetLeadByIdForUpdateAsync(leadId, cancellationToken)
            ?? throw new AppException(ErrorCodes.NotFound, "The requested lead could not be found.", 404);
    }

    protected void EnsureLeadReadyForInitialConversion(Domain.Entities.Lead lead)
    {
        if (!lead.AssignedUserId.HasValue)
        {
            throw new AppException(ErrorCodes.LeadAssignmentRequired, "A lead must be assigned before it can be qualified or converted.", 409);
        }

        if (lead.LeadStatus != LeadStatus.Qualified)
        {
            throw new AppException(ErrorCodes.InvalidStatusTransition, "Only qualified leads can be converted for the first time.", 409);
        }
    }

    protected string ResolveActor(string fallback)
    {
        return LeadManagementSupport.ResolveActor(CurrentUserContext, fallback);
    }

    protected async Task<ResolvedLeadConversionDetails> ResolveLeadConversionDetailsAsync(
        Domain.Entities.Lead lead,
        ConvertLeadToBookingCommand request,
        CancellationToken cancellationToken)
    {
        return await ResolveLeadConversionDetailsAsync(
            lead,
            new LeadConversionInput(
                request.ServiceId,
                request.AcTypeId,
                request.TonnageId,
                request.BrandId,
                request.SlotAvailabilityId,
                request.AddressLine1,
                request.AddressLine2,
                request.CityName,
                request.Pincode,
                request.InquiryNotes),
            cancellationToken);
    }

    protected async Task<ResolvedLeadConversionDetails> ResolveLeadConversionDetailsAsync(
        Domain.Entities.Lead lead,
        ConvertLeadToServiceRequestCommand request,
        CancellationToken cancellationToken)
    {
        return await ResolveLeadConversionDetailsAsync(
            lead,
            new LeadConversionInput(
                request.ServiceId,
                request.AcTypeId,
                request.TonnageId,
                request.BrandId,
                request.SlotAvailabilityId,
                request.AddressLine1,
                request.AddressLine2,
                request.CityName,
                request.Pincode,
                request.InquiryNotes),
            cancellationToken);
    }

    private async Task<ResolvedLeadConversionDetails> ResolveLeadConversionDetailsAsync(
        Domain.Entities.Lead lead,
        LeadConversionInput request,
        CancellationToken cancellationToken)
    {
        var serviceId = request.ServiceId ?? lead.ServiceId;
        var acTypeId = request.AcTypeId ?? lead.AcTypeId;
        var tonnageId = request.TonnageId ?? lead.TonnageId;
        var brandId = request.BrandId ?? lead.BrandId;
        var slotAvailabilityId = request.SlotAvailabilityId ?? lead.SlotAvailabilityId;
        var addressLine1 = request.AddressLine1?.Trim() ?? lead.AddressLine1;
        var addressLine2 = request.AddressLine2?.Trim() ?? lead.AddressLine2;
        var cityName = request.CityName?.Trim() ?? lead.CityName;
        var pincode = request.Pincode?.Trim() ?? lead.Pincode;
        var inquiryNotes = request.InquiryNotes?.Trim() ?? lead.InquiryNotes;

        if (!serviceId.HasValue || !acTypeId.HasValue || !tonnageId.HasValue || !brandId.HasValue || !slotAvailabilityId.HasValue ||
            string.IsNullOrWhiteSpace(addressLine1) || string.IsNullOrWhiteSpace(cityName) || string.IsNullOrWhiteSpace(pincode))
        {
            throw new AppException(
                ErrorCodes.LeadConversionMissingDetails,
                "Lead conversion requires service, slot, and address details.",
                400);
        }

        var service = await _bookingLookupRepository.GetServiceByIdAsync(serviceId.Value, cancellationToken)
            ?? throw new AppException(ErrorCodes.InvalidMasterSelection, "The selected service is invalid.", 400);
        var acType = await _bookingLookupRepository.GetAcTypeByIdAsync(acTypeId.Value, cancellationToken)
            ?? throw new AppException(ErrorCodes.InvalidMasterSelection, "The selected AC type is invalid.", 400);
        var tonnage = await _bookingLookupRepository.GetTonnageByIdAsync(tonnageId.Value, cancellationToken)
            ?? throw new AppException(ErrorCodes.InvalidMasterSelection, "The selected tonnage is invalid.", 400);
        var brand = await _bookingLookupRepository.GetBrandByIdAsync(brandId.Value, cancellationToken)
            ?? throw new AppException(ErrorCodes.InvalidMasterSelection, "The selected brand is invalid.", 400);
        var zone = await _bookingLookupRepository.GetZoneByPincodeAsync(pincode, cancellationToken)
            ?? throw new AppException(ErrorCodes.ZoneNotServed, "The provided pincode is not serviceable.", 404);
        var slotAvailability = await _bookingLookupRepository.GetSlotAvailabilityByIdAsync(slotAvailabilityId.Value, cancellationToken)
            ?? throw new AppException(ErrorCodes.SlotUnavailable, "The selected slot is unavailable.", 409);

        await _validationService.ValidateBookingWindowAsync(slotAvailability, lead.MobileNumber, service.ServiceId, cancellationToken);

        return new ResolvedLeadConversionDetails(
            serviceId.Value,
            acTypeId.Value,
            tonnageId.Value,
            brandId.Value,
            slotAvailabilityId.Value,
            service,
            acType,
            tonnage,
            brand,
            zone,
            slotAvailability,
            addressLine1,
            addressLine2 ?? string.Empty,
            cityName,
            pincode,
            inquiryNotes ?? string.Empty);
    }

    protected async Task<DomainBooking> CreateBookingAsync(
        Domain.Entities.Lead lead,
        ResolvedLeadConversionDetails details,
        string actor,
        DateTime now,
        CancellationToken cancellationToken)
    {
        var customer = await BookingRepository.GetCustomerByMobileAsync(lead.MobileNumber, cancellationToken);

        if (customer is null)
        {
            customer = new Customer
            {
                CustomerName = lead.CustomerName,
                MobileNumber = lead.MobileNumber,
                EmailAddress = lead.EmailAddress,
                IsGuestCustomer = false,
                CreatedBy = actor,
                DateCreated = now,
                IPAddress = CurrentUserContext.IPAddress
            };

            await BookingRepository.AddCustomerAsync(customer, cancellationToken);
        }
        else
        {
            customer.CustomerName = lead.CustomerName;
            customer.EmailAddress = lead.EmailAddress;
            customer.UpdatedBy = actor;
            customer.LastUpdated = now;
        }

        var customerAddress = await BookingRepository.GetCustomerAddressAsync(customer.CustomerId, details.AddressLine1, details.Pincode, cancellationToken);

        if (customerAddress is null)
        {
            customerAddress = new CustomerAddress
            {
                Customer = customer,
                ZoneId = details.Zone.ZoneId,
                AddressLabel = "Lead Conversion Address",
                AddressLine1 = details.AddressLine1,
                AddressLine2 = details.AddressLine2,
                CityName = details.CityName,
                Pincode = details.Pincode,
                IsDefault = customer.CustomerAddresses.Count == 0,
                CreatedBy = actor,
                DateCreated = now,
                IPAddress = CurrentUserContext.IPAddress
            };

            await BookingRepository.AddCustomerAddressAsync(customerAddress, cancellationToken);
        }
        else
        {
            customerAddress.ZoneId = details.Zone.ZoneId;
            customerAddress.AddressLine2 = details.AddressLine2;
            customerAddress.CityName = details.CityName;
            customerAddress.UpdatedBy = actor;
            customerAddress.LastUpdated = now;
        }

        details.SlotAvailability.ReservedCapacity += 1;
        var bookingReference = await GenerateUniqueBookingReferenceAsync(cancellationToken);
        var booking = new DomainBooking
        {
            BookingReference = bookingReference,
            Customer = customer,
            CustomerAddress = customerAddress,
            ZoneId = details.Zone.ZoneId,
            SlotAvailability = details.SlotAvailability,
            BookingDateUtc = now,
            BookingStatus = BookingStatus.Confirmed,
            SourceChannel = LeadManagementSupport.MapLeadSource(lead.SourceChannel),
            CustomerNameSnapshot = customer.CustomerName,
            MobileNumberSnapshot = customer.MobileNumber,
            EmailAddressSnapshot = customer.EmailAddress,
            AddressLine1Snapshot = customerAddress.AddressLine1,
            AddressLine2Snapshot = customerAddress.AddressLine2,
            LandmarkSnapshot = customerAddress.Landmark,
            CityNameSnapshot = customerAddress.CityName,
            PincodeSnapshot = customerAddress.Pincode,
            ZoneNameSnapshot = details.Zone.ZoneName,
            ServiceNameSnapshot = details.Service.ServiceName,
            EstimatedPrice = details.Service.BasePrice,
            CreatedBy = actor,
            DateCreated = now,
            IPAddress = CurrentUserContext.IPAddress
        };

        booking.BookingLines.Add(new BookingLine
        {
            ServiceId = details.Service.ServiceId,
            AcTypeId = details.AcType.AcTypeId,
            TonnageId = details.Tonnage.TonnageId,
            BrandId = details.Brand.BrandId,
            Quantity = 1,
            ModelName = "Lead Conversion",
            IssueNotes = details.InquiryNotes,
            UnitPrice = details.Service.BasePrice,
            LineTotal = details.Service.BasePrice,
            CreatedBy = actor,
            DateCreated = now,
            IPAddress = CurrentUserContext.IPAddress
        });

        booking.BookingStatusHistories.Add(new BookingStatusHistory
        {
            BookingStatus = BookingStatus.Confirmed,
            Remarks = $"Booking created from lead {lead.LeadNumber}.",
            StatusDateUtc = now,
            CreatedBy = actor,
            DateCreated = now,
            IPAddress = CurrentUserContext.IPAddress
        });

        await BookingRepository.AddBookingAsync(booking, cancellationToken);

        return booking;
    }

    protected void ApplyResolvedLeadDetails(Domain.Entities.Lead lead, ResolvedLeadConversionDetails details)
    {
        lead.ServiceId = details.ServiceId;
        lead.AcTypeId = details.AcTypeId;
        lead.TonnageId = details.TonnageId;
        lead.BrandId = details.BrandId;
        lead.SlotAvailabilityId = details.SlotAvailabilityId;
        lead.AddressLine1 = details.AddressLine1;
        lead.AddressLine2 = details.AddressLine2;
        lead.CityName = details.CityName;
        lead.Pincode = details.Pincode;
        lead.InquiryNotes = details.InquiryNotes;
    }

    protected Task WriteAuditAsync(
        string actionName,
        string entityId,
        string newValues,
        string actor,
        DateTime now,
        CancellationToken cancellationToken)
    {
        return LeadManagementSupport.WriteLeadAuditAsync(
            _auditLogRepository,
            CurrentUserContext,
            actionName,
            entityId,
            newValues,
            actor,
            now,
            cancellationToken);
    }

    private async Task<string> GenerateUniqueBookingReferenceAsync(CancellationToken cancellationToken)
    {
        while (true)
        {
            var bookingReference = _bookingReferenceGenerator.GenerateReference();

            if (!await BookingRepository.BookingReferenceExistsAsync(bookingReference, cancellationToken))
            {
                return bookingReference;
            }
        }
    }
}

internal sealed record LeadConversionInput(
    long? ServiceId,
    long? AcTypeId,
    long? TonnageId,
    long? BrandId,
    long? SlotAvailabilityId,
    string? AddressLine1,
    string? AddressLine2,
    string? CityName,
    string? Pincode,
    string? InquiryNotes);

public sealed record ResolvedLeadConversionDetails(
    long ServiceId,
    long AcTypeId,
    long TonnageId,
    long BrandId,
    long SlotAvailabilityId,
    Service Service,
    AcType AcType,
    Tonnage Tonnage,
    Brand Brand,
    Zone Zone,
    SlotAvailability SlotAvailability,
    string AddressLine1,
    string AddressLine2,
    string CityName,
    string Pincode,
    string InquiryNotes);

internal static class LeadManagementSupport
{
    public static bool TryParseLeadSourceChannel(string? rawValue)
    {
        return TryParseLeadSourceChannel(rawValue, out _);
    }

    public static bool TryParseLeadSourceChannel(string? rawValue, out LeadSourceChannel sourceChannel)
    {
        sourceChannel = LeadSourceChannel.Web;

        if (string.IsNullOrWhiteSpace(rawValue))
        {
            return false;
        }

        return Normalize(rawValue) switch
        {
            "website" or "web" => Assign(LeadSourceChannel.Web, out sourceChannel),
            "app" or "mobileapp" or "mobile" => Assign(LeadSourceChannel.MobileApp, out sourceChannel),
            "phone" or "call" => Assign(LeadSourceChannel.Phone, out sourceChannel),
            "whatsapp" => Assign(LeadSourceChannel.WhatsApp, out sourceChannel),
            "manual" or "admin" => Assign(LeadSourceChannel.Manual, out sourceChannel),
            _ => Enum.TryParse(rawValue, true, out sourceChannel)
        };
    }

    public static bool TryParseLeadStatus(string? rawValue, out LeadStatus leadStatus)
    {
        leadStatus = LeadStatus.New;

        if (string.IsNullOrWhiteSpace(rawValue))
        {
            return false;
        }

        return Normalize(rawValue) switch
        {
            "new" => Assign(LeadStatus.New, out leadStatus),
            "contacted" => Assign(LeadStatus.Contacted, out leadStatus),
            "qualified" => Assign(LeadStatus.Qualified, out leadStatus),
            "converted" => Assign(LeadStatus.Converted, out leadStatus),
            "lost" => Assign(LeadStatus.Lost, out leadStatus),
            "closed" => Assign(LeadStatus.Closed, out leadStatus),
            _ => Enum.TryParse(rawValue, true, out leadStatus)
        };
    }

    public static string ResolveActor(ICurrentUserContext currentUserContext, string fallback)
    {
        return string.IsNullOrWhiteSpace(currentUserContext.UserName)
            ? fallback
            : currentUserContext.UserName;
    }

    public static BookingSourceChannel MapLeadSource(LeadSourceChannel sourceChannel)
    {
        return sourceChannel switch
        {
            LeadSourceChannel.Phone => BookingSourceChannel.Phone,
            LeadSourceChannel.WhatsApp => BookingSourceChannel.WhatsApp,
            LeadSourceChannel.MobileApp => BookingSourceChannel.Mobile,
            LeadSourceChannel.Manual => BookingSourceChannel.Admin,
            _ => BookingSourceChannel.Web
        };
    }

    public static string ToDisplayLeadSourceChannel(LeadSourceChannel sourceChannel)
    {
        return sourceChannel switch
        {
            LeadSourceChannel.MobileApp => "App",
            LeadSourceChannel.Manual => "Manual",
            LeadSourceChannel.Phone => "Phone",
            LeadSourceChannel.WhatsApp => "WhatsApp",
            _ => "Website"
        };
    }

    public static string ToDisplayLeadStatus(LeadStatus leadStatus)
    {
        return leadStatus.ToString();
    }

    public static LeadResponse MapLead(Domain.Entities.Lead lead)
    {
        return new LeadResponse(
            lead.LeadId,
            lead.LeadNumber,
            lead.CustomerName,
            lead.MobileNumber,
            ToDisplayLeadStatus(lead.LeadStatus),
            lead.ConvertedBookingId,
            lead.ConvertedServiceRequestId);
    }

    public static LeadListItemResponse MapLeadListItem(Domain.Entities.Lead lead)
    {
        return new LeadListItemResponse(
            lead.LeadId,
            lead.LeadNumber,
            lead.CustomerName,
            lead.MobileNumber,
            string.IsNullOrWhiteSpace(lead.EmailAddress) ? null : lead.EmailAddress,
            ToDisplayLeadSourceChannel(lead.SourceChannel),
            ToDisplayLeadStatus(lead.LeadStatus),
            lead.AssignedUserId,
            lead.AssignedUser?.FullName,
            lead.DateCreated,
            lead.LastContactedDateUtc,
            lead.ConvertedDateUtc,
            string.IsNullOrWhiteSpace(lead.LostReason) ? null : lead.LostReason);
    }

    public static LeadDetailResponse MapLeadDetail(Domain.Entities.Lead lead)
    {
        return new LeadDetailResponse(
            lead.LeadId,
            lead.LeadNumber,
            lead.CustomerName,
            lead.MobileNumber,
            string.IsNullOrWhiteSpace(lead.EmailAddress) ? null : lead.EmailAddress,
            ToDisplayLeadSourceChannel(lead.SourceChannel),
            ToDisplayLeadStatus(lead.LeadStatus),
            lead.AssignedUserId,
            lead.AssignedUser?.FullName,
            lead.ServiceId,
            lead.AcTypeId,
            lead.TonnageId,
            lead.BrandId,
            lead.SlotAvailabilityId,
            lead.ConvertedBookingId,
            lead.ConvertedServiceRequestId,
            string.IsNullOrWhiteSpace(lead.AddressLine1) ? null : lead.AddressLine1,
            string.IsNullOrWhiteSpace(lead.AddressLine2) ? null : lead.AddressLine2,
            string.IsNullOrWhiteSpace(lead.CityName) ? null : lead.CityName,
            string.IsNullOrWhiteSpace(lead.Pincode) ? null : lead.Pincode,
            string.IsNullOrWhiteSpace(lead.InquiryNotes) ? null : lead.InquiryNotes,
            string.IsNullOrWhiteSpace(lead.LostReason) ? null : lead.LostReason,
            lead.DateCreated,
            lead.LastContactedDateUtc,
            lead.ConvertedDateUtc,
            lead.ClosedDateUtc,
            lead.StatusHistories
                .Where(history => !history.IsDeleted)
                .OrderByDescending(history => history.ChangedDateUtc)
                .Select(
                    history => new LeadStatusHistoryResponse(
                        history.LeadStatusHistoryId,
                        ToDisplayLeadStatus(history.PreviousStatus),
                        ToDisplayLeadStatus(history.CurrentStatus),
                        history.Remarks,
                        history.CreatedBy,
                        history.ChangedDateUtc))
                .ToArray(),
            lead.Assignments
                .Where(assignment => !assignment.IsDeleted)
                .OrderByDescending(assignment => assignment.AssignedDateUtc)
                .Select(
                    assignment => new LeadAssignmentResponse(
                        assignment.LeadAssignmentId,
                        assignment.AssignedUserId,
                        assignment.AssignedUser?.FullName ?? $"User #{assignment.AssignedUserId}",
                        assignment.PreviousAssignedUserId,
                        assignment.Remarks,
                        assignment.CreatedBy,
                        assignment.AssignedDateUtc))
                .ToArray(),
            lead.Notes
                .Where(note => !note.IsDeleted)
                .OrderByDescending(note => note.NoteDateUtc)
                .Select(
                    note => new LeadNoteResponse(
                        note.LeadNoteId,
                        note.NoteText,
                        note.IsInternal,
                        note.CreatedBy,
                        note.NoteDateUtc))
                .ToArray(),
            lead.Conversions
                .Where(conversion => !conversion.IsDeleted)
                .OrderByDescending(conversion => conversion.ConvertedDateUtc)
                .Select(
                    conversion => new LeadConversionResponse(
                        conversion.LeadConversionId,
                        conversion.ConversionType.ToString(),
                        conversion.BookingId,
                        conversion.ServiceRequestId,
                        conversion.ReferenceNumber,
                        conversion.Remarks,
                        conversion.ConvertedDateUtc))
                .ToArray());
    }

    public static Task WriteLeadAuditAsync(
        IAuditLogRepository auditLogRepository,
        ICurrentUserContext currentUserContext,
        string actionName,
        string entityId,
        string newValues,
        string actor,
        DateTime now,
        CancellationToken cancellationToken)
    {
        return auditLogRepository.AddAsync(
            new AuditLog
            {
                UserId = currentUserContext.UserId,
                ActionName = actionName,
                EntityName = nameof(Domain.Entities.Lead),
                EntityId = entityId,
                TraceId = currentUserContext.TraceId,
                StatusName = "Success",
                NewValues = newValues,
                CreatedBy = actor,
                DateCreated = now,
                IPAddress = currentUserContext.IPAddress
            },
            cancellationToken);
    }

    private static string Normalize(string rawValue)
    {
        return rawValue
            .Trim()
            .Replace(" ", string.Empty, StringComparison.OrdinalIgnoreCase)
            .Replace("-", string.Empty, StringComparison.OrdinalIgnoreCase)
            .Replace("_", string.Empty, StringComparison.OrdinalIgnoreCase)
            .ToLowerInvariant();
    }

    private static bool Assign<T>(T value, out T target)
    {
        target = value;
        return true;
    }
}
