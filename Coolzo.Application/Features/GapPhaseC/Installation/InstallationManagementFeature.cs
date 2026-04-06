using Coolzo.Application.Common.Interfaces;
using Coolzo.Application.Common.Services;
using Coolzo.Contracts.Common;
using Coolzo.Contracts.Responses.GapPhaseC;
using Coolzo.Domain.Entities;
using Coolzo.Domain.Enums;
using Coolzo.Shared.Constants;
using Coolzo.Shared.Exceptions;
using Coolzo.Shared.Models;
using FluentValidation;
using MediatR;

namespace Coolzo.Application.Features.GapPhaseC.Installation;

public sealed record CreateInstallationCommand(
    long? LeadId,
    string CustomerName,
    string MobileNumber,
    string? EmailAddress,
    string SourceChannel,
    string AddressLine1,
    string? AddressLine2,
    string CityName,
    string Pincode,
    string InstallationType,
    int NumberOfUnits,
    string? SiteNotes,
    DateTime? PreferredSurveyDateUtc) : IRequest<InstallationSummaryResponse>;

public sealed class CreateInstallationCommandValidator : AbstractValidator<CreateInstallationCommand>
{
    public CreateInstallationCommandValidator()
    {
        RuleFor(request => request.CustomerName).NotEmpty().MaximumLength(128);
        RuleFor(request => request.MobileNumber).NotEmpty().MaximumLength(32);
        RuleFor(request => request.EmailAddress).MaximumLength(128);
        RuleFor(request => request.SourceChannel).NotEmpty().MaximumLength(32);
        RuleFor(request => request.AddressLine1).NotEmpty().MaximumLength(256);
        RuleFor(request => request.AddressLine2).MaximumLength(256);
        RuleFor(request => request.CityName).NotEmpty().MaximumLength(128);
        RuleFor(request => request.Pincode).NotEmpty().MaximumLength(16);
        RuleFor(request => request.InstallationType).NotEmpty().MaximumLength(64);
        RuleFor(request => request.NumberOfUnits).GreaterThan(0).LessThanOrEqualTo(250);
        RuleFor(request => request.SiteNotes).MaximumLength(512);
    }
}

public sealed class CreateInstallationCommandHandler : IRequestHandler<CreateInstallationCommand, InstallationSummaryResponse>
{
    private static readonly TimeSpan DuplicateLeadWindow = TimeSpan.FromDays(14);

    private readonly IAuditLogRepository _auditLogRepository;
    private readonly IBookingLookupRepository _bookingLookupRepository;
    private readonly IBookingRepository _bookingRepository;
    private readonly ICurrentDateTime _currentDateTime;
    private readonly ICurrentUserContext _currentUserContext;
    private readonly IGapPhaseAReferenceGenerator _gapPhaseAReferenceGenerator;
    private readonly IGapPhaseARepository _gapPhaseARepository;
    private readonly IInstallationLifecycleReferenceGenerator _installationLifecycleReferenceGenerator;
    private readonly IInstallationLifecycleRepository _installationLifecycleRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateInstallationCommandHandler(
        IInstallationLifecycleRepository installationLifecycleRepository,
        IInstallationLifecycleReferenceGenerator installationLifecycleReferenceGenerator,
        IGapPhaseARepository gapPhaseARepository,
        IGapPhaseAReferenceGenerator gapPhaseAReferenceGenerator,
        IBookingRepository bookingRepository,
        IBookingLookupRepository bookingLookupRepository,
        IAuditLogRepository auditLogRepository,
        IUnitOfWork unitOfWork,
        ICurrentDateTime currentDateTime,
        ICurrentUserContext currentUserContext)
    {
        _installationLifecycleRepository = installationLifecycleRepository;
        _installationLifecycleReferenceGenerator = installationLifecycleReferenceGenerator;
        _gapPhaseARepository = gapPhaseARepository;
        _gapPhaseAReferenceGenerator = gapPhaseAReferenceGenerator;
        _bookingRepository = bookingRepository;
        _bookingLookupRepository = bookingLookupRepository;
        _auditLogRepository = auditLogRepository;
        _unitOfWork = unitOfWork;
        _currentDateTime = currentDateTime;
        _currentUserContext = currentUserContext;
    }

    public async Task<InstallationSummaryResponse> Handle(CreateInstallationCommand request, CancellationToken cancellationToken)
    {
        var now = _currentDateTime.UtcNow;
        var actorName = InstallationLifecycleSupport.ResolveActorName(_currentUserContext, "InstallationLead");
        var lead = await ResolveLeadAsync(request, actorName, now, cancellationToken);
        var customer = await ResolveCustomerAsync(request, actorName, now, cancellationToken);
        var customerAddress = await ResolveCustomerAddressAsync(request, customer, actorName, now, cancellationToken);

        var installation = new InstallationLead
        {
            InstallationNumber = await GenerateInstallationNumberAsync(cancellationToken),
            LeadId = lead?.LeadId,
            Lead = lead,
            Customer = customer,
            CustomerAddress = customerAddress,
            CustomerId = customer.CustomerId,
            CustomerAddressId = customerAddress.CustomerAddressId,
            NumberOfUnits = request.NumberOfUnits,
            InstallationType = request.InstallationType.Trim(),
            SiteNotes = request.SiteNotes?.Trim() ?? string.Empty,
            SurveyDateUtc = request.PreferredSurveyDateUtc,
            ApprovalStatus = InstallationApprovalStatus.Pending,
            InstallationStatus = InstallationLifecycleStatus.LeadCreated,
            CreatedBy = actorName,
            DateCreated = now,
            IPAddress = _currentUserContext.IPAddress
        };

        installation.StatusHistories.Add(new InstallationStatusHistory
        {
            PreviousStatus = InstallationLifecycleStatus.LeadCreated,
            CurrentStatus = InstallationLifecycleStatus.LeadCreated,
            Remarks = "Installation lead created.",
            ChangedByRole = InstallationLifecycleSupport.ResolveActorRole(_currentUserContext),
            ChangedDateUtc = now,
            CreatedBy = actorName,
            DateCreated = now,
            IPAddress = _currentUserContext.IPAddress
        });

        await _installationLifecycleRepository.AddInstallationAsync(installation, cancellationToken);
        await _auditLogRepository.AddAsync(
            new AuditLog
            {
                UserId = _currentUserContext.UserId,
                ActionName = "CreateInstallationLead",
                EntityName = nameof(InstallationLead),
                EntityId = installation.InstallationNumber,
                TraceId = _currentUserContext.TraceId,
                StatusName = "Success",
                NewValues = installation.InstallationType,
                CreatedBy = actorName,
                DateCreated = now,
                IPAddress = _currentUserContext.IPAddress
            },
            cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return InstallationLifecycleSupport.MapSummary(installation);
    }

    private async Task<Lead?> ResolveLeadAsync(CreateInstallationCommand request, string actorName, DateTime now, CancellationToken cancellationToken)
    {
        if (request.LeadId.HasValue)
        {
            return await _gapPhaseARepository.GetLeadByIdForUpdateAsync(request.LeadId.Value, cancellationToken)
                ?? throw new AppException(ErrorCodes.NotFound, "The linked lead could not be found.", 404);
        }

        var normalizedMobile = request.MobileNumber.Trim();

        if (await _gapPhaseARepository.ActiveLeadExistsByMobileAsync(normalizedMobile, now.Subtract(DuplicateLeadWindow), cancellationToken))
        {
            throw new AppException(
                ErrorCodes.LeadDuplicateDetected,
                "An active lead already exists for this mobile number. Link the existing lead before creating a new installation.",
                409);
        }

        InstallationLifecycleSupport.ParseLeadSourceChannel(request.SourceChannel, out var sourceChannel);

        var lead = new Lead
        {
            LeadNumber = await GenerateLeadNumberAsync(cancellationToken),
            CustomerName = request.CustomerName.Trim(),
            MobileNumber = normalizedMobile,
            EmailAddress = request.EmailAddress?.Trim() ?? string.Empty,
            SourceChannel = sourceChannel,
            LeadStatus = LeadStatus.New,
            AddressLine1 = request.AddressLine1.Trim(),
            AddressLine2 = request.AddressLine2?.Trim() ?? string.Empty,
            CityName = request.CityName.Trim(),
            Pincode = request.Pincode.Trim(),
            InquiryNotes = request.SiteNotes?.Trim() ?? $"Installation request for {request.InstallationType.Trim()}",
            CreatedBy = actorName,
            DateCreated = now,
            IPAddress = _currentUserContext.IPAddress
        };

        lead.StatusHistories.Add(new LeadStatusHistory
        {
            PreviousStatus = LeadStatus.New,
            CurrentStatus = LeadStatus.New,
            Remarks = "Lead captured from installation request.",
            ChangedDateUtc = now,
            CreatedBy = actorName,
            DateCreated = now,
            IPAddress = _currentUserContext.IPAddress
        });

        await _gapPhaseARepository.AddLeadAsync(lead, cancellationToken);

        return lead;
    }

    private async Task<Customer> ResolveCustomerAsync(CreateInstallationCommand request, string actorName, DateTime now, CancellationToken cancellationToken)
    {
        Customer? customer = null;

        if (_currentUserContext.IsAuthenticated && _currentUserContext.UserId.HasValue)
        {
            customer = await _bookingRepository.GetCustomerByUserIdAsync(_currentUserContext.UserId.Value, cancellationToken);
        }

        customer ??= await _bookingRepository.GetCustomerByMobileAsync(request.MobileNumber.Trim(), cancellationToken);

        if (customer is null)
        {
            customer = new Customer
            {
                UserId = _currentUserContext.IsAuthenticated ? _currentUserContext.UserId : null,
                CustomerName = request.CustomerName.Trim(),
                MobileNumber = request.MobileNumber.Trim(),
                EmailAddress = request.EmailAddress?.Trim() ?? string.Empty,
                IsGuestCustomer = !_currentUserContext.Roles.Contains(RoleNames.Customer),
                IsActive = true,
                CreatedBy = actorName,
                DateCreated = now,
                IPAddress = _currentUserContext.IPAddress
            };

            await _bookingRepository.AddCustomerAsync(customer, cancellationToken);
            return customer;
        }

        customer.CustomerName = request.CustomerName.Trim();
        customer.EmailAddress = request.EmailAddress?.Trim() ?? customer.EmailAddress;
        customer.UserId ??= _currentUserContext.IsAuthenticated ? _currentUserContext.UserId : null;
        customer.LastUpdated = now;
        customer.UpdatedBy = actorName;

        return customer;
    }

    private async Task<CustomerAddress> ResolveCustomerAddressAsync(
        CreateInstallationCommand request,
        Customer customer,
        string actorName,
        DateTime now,
        CancellationToken cancellationToken)
    {
        var zone = await _bookingLookupRepository.GetZoneByPincodeAsync(request.Pincode.Trim(), cancellationToken)
            ?? throw new AppException(ErrorCodes.ZoneNotServed, "The provided pincode is not serviceable for installation requests.", 404);

        var customerAddress = await _bookingRepository.GetCustomerAddressAsync(
            customer.CustomerId,
            request.AddressLine1.Trim(),
            request.Pincode.Trim(),
            cancellationToken);

        if (customerAddress is null)
        {
            customerAddress = new CustomerAddress
            {
                Customer = customer,
                ZoneId = zone.ZoneId,
                AddressLabel = "Installation Address",
                AddressLine1 = request.AddressLine1.Trim(),
                AddressLine2 = request.AddressLine2?.Trim() ?? string.Empty,
                CityName = request.CityName.Trim(),
                Pincode = request.Pincode.Trim(),
                IsDefault = customer.CustomerAddresses.Count == 0,
                IsActive = true,
                CreatedBy = actorName,
                DateCreated = now,
                IPAddress = _currentUserContext.IPAddress
            };

            await _bookingRepository.AddCustomerAddressAsync(customerAddress, cancellationToken);
            return customerAddress;
        }

        customerAddress.ZoneId = zone.ZoneId;
        customerAddress.AddressLine2 = request.AddressLine2?.Trim() ?? string.Empty;
        customerAddress.CityName = request.CityName.Trim();
        customerAddress.LastUpdated = now;
        customerAddress.UpdatedBy = actorName;

        return customerAddress;
    }

    private async Task<string> GenerateInstallationNumberAsync(CancellationToken cancellationToken)
    {
        while (true)
        {
            var installationNumber = _installationLifecycleReferenceGenerator.GenerateInstallationNumber();

            if (!await _installationLifecycleRepository.InstallationNumberExistsAsync(installationNumber, cancellationToken))
            {
                return installationNumber;
            }
        }
    }

    private async Task<string> GenerateLeadNumberAsync(CancellationToken cancellationToken)
    {
        while (true)
        {
            var leadNumber = _gapPhaseAReferenceGenerator.GenerateLeadNumber();

            if (!await _gapPhaseARepository.LeadNumberExistsAsync(leadNumber, cancellationToken))
            {
                return leadNumber;
            }
        }
    }
}

public sealed record GetInstallationListQuery(
    string? SearchTerm,
    string? InstallationStatus,
    string? ApprovalStatus,
    int PageNumber,
    int PageSize) : IRequest<PagedResult<InstallationListItemResponse>>;

public sealed class GetInstallationListQueryValidator : AbstractValidator<GetInstallationListQuery>
{
    public GetInstallationListQueryValidator()
    {
        RuleFor(request => request.PageNumber).GreaterThan(0);
        RuleFor(request => request.PageSize).InclusiveBetween(1, 100);
        RuleFor(request => request.SearchTerm).MaximumLength(128);
        RuleFor(request => request.InstallationStatus).MaximumLength(64);
        RuleFor(request => request.ApprovalStatus).MaximumLength(32);
    }
}

public sealed class GetInstallationListQueryHandler : IRequestHandler<GetInstallationListQuery, PagedResult<InstallationListItemResponse>>
{
    private readonly InstallationLifecycleAccessService _accessService;
    private readonly IInstallationLifecycleRepository _installationLifecycleRepository;

    public GetInstallationListQueryHandler(
        IInstallationLifecycleRepository installationLifecycleRepository,
        InstallationLifecycleAccessService accessService)
    {
        _installationLifecycleRepository = installationLifecycleRepository;
        _accessService = accessService;
    }

    public async Task<PagedResult<InstallationListItemResponse>> Handle(GetInstallationListQuery request, CancellationToken cancellationToken)
    {
        var customerId = await _accessService.GetScopedCustomerIdAsync(cancellationToken);
        var technicianId = await _accessService.GetScopedTechnicianIdAsync(cancellationToken);
        var installationStatus = TryParseStatus<InstallationLifecycleStatus>(request.InstallationStatus);
        var approvalStatus = TryParseStatus<InstallationApprovalStatus>(request.ApprovalStatus);

        var installations = await _installationLifecycleRepository.SearchInstallationsAsync(
            request.SearchTerm,
            installationStatus,
            approvalStatus,
            customerId,
            technicianId,
            request.PageNumber,
            request.PageSize,
            cancellationToken);

        var totalCount = await _installationLifecycleRepository.CountInstallationsAsync(
            request.SearchTerm,
            installationStatus,
            approvalStatus,
            customerId,
            technicianId,
            cancellationToken);

        return new PagedResult<InstallationListItemResponse>(
            installations.Select(InstallationLifecycleSupport.MapListItem).ToArray(),
            totalCount,
            request.PageNumber,
            request.PageSize);
    }

    private static TEnum? TryParseStatus<TEnum>(string? rawStatus)
        where TEnum : struct, Enum
    {
        return Enum.TryParse<TEnum>(rawStatus, true, out var parsedStatus) ? parsedStatus : null;
    }
}

public sealed record GetInstallationDetailQuery(long InstallationId) : IRequest<InstallationDetailResponse>;

public sealed class GetInstallationDetailQueryValidator : AbstractValidator<GetInstallationDetailQuery>
{
    public GetInstallationDetailQueryValidator()
    {
        RuleFor(request => request.InstallationId).GreaterThan(0);
    }
}

public sealed class GetInstallationDetailQueryHandler : IRequestHandler<GetInstallationDetailQuery, InstallationDetailResponse>
{
    private readonly InstallationLifecycleAccessService _accessService;
    private readonly IInstallationLifecycleRepository _installationLifecycleRepository;

    public GetInstallationDetailQueryHandler(
        IInstallationLifecycleRepository installationLifecycleRepository,
        InstallationLifecycleAccessService accessService)
    {
        _installationLifecycleRepository = installationLifecycleRepository;
        _accessService = accessService;
    }

    public async Task<InstallationDetailResponse> Handle(GetInstallationDetailQuery request, CancellationToken cancellationToken)
    {
        var installation = await _installationLifecycleRepository.GetInstallationByIdAsync(request.InstallationId, cancellationToken)
            ?? throw new AppException(ErrorCodes.NotFound, "The installation could not be found.", 404);

        await _accessService.EnsureReadAccessAsync(installation, cancellationToken);

        return InstallationLifecycleSupport.MapDetail(installation);
    }
}
