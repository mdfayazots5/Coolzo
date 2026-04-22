using Coolzo.Application.Common.Interfaces;
using Coolzo.Application.Common.Models;
using Coolzo.Application.Features.CustomerApp;
using Coolzo.Contracts.Common;
using Coolzo.Contracts.Responses.Customer;
using Coolzo.Domain.Entities;
using Coolzo.Shared.Constants;
using Coolzo.Shared.Exceptions;
using Coolzo.Shared.Models;
using FluentValidation;
using MediatR;

namespace Coolzo.Application.Features.CustomerManagement;

public sealed record GetAdminCustomerListQuery(
    string? SearchTerm,
    int PageNumber,
    int PageSize) : IRequest<PagedResult<CustomerAdminListItemResponse>>;

public sealed record GetAdminCustomerDetailQuery(long CustomerId) : IRequest<CustomerAdminDetailResponse>;

public sealed record UpdateAdminCustomerCommand(
    long CustomerId,
    string CustomerName,
    string MobileNumber,
    string EmailAddress) : IRequest<CustomerAdminDetailResponse>;

public sealed record CreateAdminCustomerAddressCommand(
    long CustomerId,
    string AddressLabel,
    string AddressLine1,
    string AddressLine2,
    string Landmark,
    string CityName,
    string Pincode,
    long? ZoneId,
    double? Latitude,
    double? Longitude,
    bool IsDefault,
    string? StateName,
    string? AddressType) : IRequest<CustomerAddressResponse>;

public sealed record UpdateAdminCustomerAddressCommand(
    long CustomerId,
    long AddressId,
    string AddressLabel,
    string AddressLine1,
    string AddressLine2,
    string Landmark,
    string CityName,
    string Pincode,
    long? ZoneId,
    double? Latitude,
    double? Longitude,
    bool IsDefault,
    string? StateName,
    string? AddressType) : IRequest<CustomerAddressResponse>;

public sealed record CreateAdminCustomerEquipmentCommand(
    long CustomerId,
    string Name,
    string Type,
    string Brand,
    string Capacity,
    string Location,
    DateOnly? PurchaseDate,
    DateOnly? LastServiceDate,
    string? SerialNumber) : IRequest<CustomerEquipmentResponse>;

public sealed record UpdateAdminCustomerEquipmentCommand(
    long CustomerId,
    long CustomerEquipmentId,
    string Name,
    string Type,
    string Brand,
    string Capacity,
    string Location,
    DateOnly? PurchaseDate,
    DateOnly? LastServiceDate,
    string? SerialNumber) : IRequest<CustomerEquipmentResponse>;

public sealed record CreateCustomerNoteCommand(
    long CustomerId,
    string Content,
    bool IsPrivate,
    string? NoteType) : IRequest<CustomerNoteResponse>;

internal static class CustomerManagementMapper
{
    public static CustomerAdminListItemResponse ToListResponse(CustomerManagementListItemView view)
    {
        return new CustomerAdminListItemResponse(
            view.CustomerId,
            view.CustomerName,
            view.MobileNumber,
            view.EmailAddress,
            view.IsActive,
            ResolveRiskLevel(view.OpenSupportTicketCount, view.OutstandingAmount),
            view.TotalServicesCount,
            view.TotalRevenueAmount,
            view.OutstandingAmount,
            view.HasActiveAmc,
            view.OpenSupportTicketCount,
            view.CustomerSinceUtc,
            view.LastServiceDateUtc,
            view.PrimaryAddressSummary);
    }

    public static CustomerAdminDetailResponse ToDetailResponse(
        CustomerManagementDetailView view,
        IReadOnlyCollection<CustomerAddressResponse> addresses,
        IReadOnlyCollection<CustomerEquipmentResponse> equipment,
        IReadOnlyCollection<CustomerNoteResponse> notes)
    {
        return new CustomerAdminDetailResponse(
            view.CustomerId,
            view.CustomerName,
            view.MobileNumber,
            view.EmailAddress,
            view.IsActive,
            ResolveRiskLevel(view.OpenSupportTicketCount, view.OutstandingAmount),
            view.TotalServicesCount,
            view.TotalRevenueAmount,
            view.OutstandingAmount,
            view.HasActiveAmc,
            view.OpenSupportTicketCount,
            view.TotalSupportTicketCount,
            view.CustomerSinceUtc,
            view.LastServiceDateUtc,
            view.LastInvoiceDateUtc,
            view.LastInvoiceStatus?.ToString(),
            view.PrimaryAddressSummary,
            view.ActiveAmcCount,
            view.ActiveAmcPlanName,
            view.ActiveAmcStatus?.ToString(),
            view.VisitsIncluded,
            view.VisitsUsed,
            view.NextAmcVisitDate,
            addresses,
            equipment,
            notes);
    }

    public static CustomerNoteResponse ToNoteResponse(AuditLog auditLog)
    {
        return new CustomerNoteResponse(
            auditLog.AuditLogId.ToString(),
            string.IsNullOrWhiteSpace(auditLog.CreatedBy) ? "System" : auditLog.CreatedBy,
            auditLog.NewValues ?? string.Empty,
            auditLog.DateCreated,
            string.Equals(auditLog.StatusName, "Private", StringComparison.OrdinalIgnoreCase),
            string.IsNullOrWhiteSpace(auditLog.Comments) ? "Internal" : auditLog.Comments);
    }

    private static string ResolveRiskLevel(int openSupportTicketCount, decimal outstandingAmount)
    {
        if (openSupportTicketCount >= 2 || outstandingAmount > 0)
        {
            return "high";
        }

        if (openSupportTicketCount == 1)
        {
            return "medium";
        }

        return "low";
    }
}

public sealed class GetAdminCustomerListQueryValidator : AbstractValidator<GetAdminCustomerListQuery>
{
    public GetAdminCustomerListQueryValidator()
    {
        RuleFor(request => request.PageNumber).GreaterThan(0);
        RuleFor(request => request.PageSize).InclusiveBetween(1, 100);
    }
}

public sealed class GetAdminCustomerDetailQueryValidator : AbstractValidator<GetAdminCustomerDetailQuery>
{
    public GetAdminCustomerDetailQueryValidator()
    {
        RuleFor(request => request.CustomerId).GreaterThan(0);
    }
}

public sealed class UpdateAdminCustomerCommandValidator : AbstractValidator<UpdateAdminCustomerCommand>
{
    public UpdateAdminCustomerCommandValidator()
    {
        RuleFor(request => request.CustomerId).GreaterThan(0);
        RuleFor(request => request.CustomerName).NotEmpty().MaximumLength(128);
        RuleFor(request => request.MobileNumber).NotEmpty().MaximumLength(32);
        RuleFor(request => request.EmailAddress).NotEmpty().EmailAddress().MaximumLength(128);
    }
}

public sealed class CreateCustomerNoteCommandValidator : AbstractValidator<CreateCustomerNoteCommand>
{
    public CreateCustomerNoteCommandValidator()
    {
        RuleFor(request => request.CustomerId).GreaterThan(0);
        RuleFor(request => request.Content).NotEmpty().MaximumLength(2000);
        RuleFor(request => request.NoteType).MaximumLength(64);
    }
}

public sealed class GetAdminCustomerListQueryHandler : IRequestHandler<GetAdminCustomerListQuery, PagedResult<CustomerAdminListItemResponse>>
{
    private readonly ICustomerManagementRepository _customerManagementRepository;

    public GetAdminCustomerListQueryHandler(ICustomerManagementRepository customerManagementRepository)
    {
        _customerManagementRepository = customerManagementRepository;
    }

    public async Task<PagedResult<CustomerAdminListItemResponse>> Handle(GetAdminCustomerListQuery request, CancellationToken cancellationToken)
    {
        var customers = await _customerManagementRepository.SearchAsync(request.SearchTerm, request.PageNumber, request.PageSize, cancellationToken);
        var totalCount = await _customerManagementRepository.CountSearchAsync(request.SearchTerm, cancellationToken);

        return new PagedResult<CustomerAdminListItemResponse>(
            customers.Select(CustomerManagementMapper.ToListResponse).ToArray(),
            totalCount,
            request.PageNumber,
            request.PageSize);
    }
}

public sealed class GetAdminCustomerDetailQueryHandler : IRequestHandler<GetAdminCustomerDetailQuery, CustomerAdminDetailResponse>
{
    private readonly ICustomerManagementRepository _customerManagementRepository;
    private readonly ICustomerAppRepository _customerAppRepository;

    public GetAdminCustomerDetailQueryHandler(
        ICustomerManagementRepository customerManagementRepository,
        ICustomerAppRepository customerAppRepository)
    {
        _customerManagementRepository = customerManagementRepository;
        _customerAppRepository = customerAppRepository;
    }

    public async Task<CustomerAdminDetailResponse> Handle(GetAdminCustomerDetailQuery request, CancellationToken cancellationToken)
    {
        var detail = await _customerManagementRepository.GetDetailAsync(request.CustomerId, cancellationToken)
            ?? throw new AppException(ErrorCodes.NotFound, "The requested customer could not be found.", 404);

        var addresses = await _customerAppRepository.ListAddressesAsync(request.CustomerId, cancellationToken);
        var equipment = await _customerAppRepository.ListEquipmentAsync(request.CustomerId, cancellationToken);
        var notes = await _customerManagementRepository.ListCustomerNotesAsync(request.CustomerId, 20, cancellationToken);

        return CustomerManagementMapper.ToDetailResponse(
            detail,
            addresses.Select(CustomerAppMapper.ToAddress).ToArray(),
            equipment.Select(CustomerAppMapper.ToEquipment).ToArray(),
            notes.Select(CustomerManagementMapper.ToNoteResponse).ToArray());
    }
}

public sealed class UpdateAdminCustomerCommandHandler : IRequestHandler<UpdateAdminCustomerCommand, CustomerAdminDetailResponse>
{
    private readonly ICustomerManagementRepository _customerManagementRepository;
    private readonly ICustomerAppRepository _customerAppRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentDateTime _currentDateTime;
    private readonly ICurrentUserContext _currentUserContext;

    public UpdateAdminCustomerCommandHandler(
        ICustomerManagementRepository customerManagementRepository,
        ICustomerAppRepository customerAppRepository,
        IUnitOfWork unitOfWork,
        ICurrentDateTime currentDateTime,
        ICurrentUserContext currentUserContext)
    {
        _customerManagementRepository = customerManagementRepository;
        _customerAppRepository = customerAppRepository;
        _unitOfWork = unitOfWork;
        _currentDateTime = currentDateTime;
        _currentUserContext = currentUserContext;
    }

    public async Task<CustomerAdminDetailResponse> Handle(UpdateAdminCustomerCommand request, CancellationToken cancellationToken)
    {
        var customer = await _customerManagementRepository.GetCustomerForUpdateAsync(request.CustomerId, cancellationToken)
            ?? throw new AppException(ErrorCodes.NotFound, "The requested customer could not be found.", 404);

        customer.CustomerName = request.CustomerName.Trim();
        customer.MobileNumber = request.MobileNumber.Trim();
        customer.EmailAddress = request.EmailAddress.Trim();
        customer.LastUpdated = _currentDateTime.UtcNow;
        customer.UpdatedBy = ResolveActor();

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var detail = await _customerManagementRepository.GetDetailAsync(customer.CustomerId, cancellationToken)
            ?? throw new AppException(ErrorCodes.NotFound, "The requested customer could not be found.", 404);
        var addresses = await _customerAppRepository.ListAddressesAsync(customer.CustomerId, cancellationToken);
        var equipment = await _customerAppRepository.ListEquipmentAsync(customer.CustomerId, cancellationToken);
        var notes = await _customerManagementRepository.ListCustomerNotesAsync(customer.CustomerId, 20, cancellationToken);

        return CustomerManagementMapper.ToDetailResponse(
            detail,
            addresses.Select(CustomerAppMapper.ToAddress).ToArray(),
            equipment.Select(CustomerAppMapper.ToEquipment).ToArray(),
            notes.Select(CustomerManagementMapper.ToNoteResponse).ToArray());
    }

    private string ResolveActor()
    {
        return string.IsNullOrWhiteSpace(_currentUserContext.UserName) ? "CustomerManagement" : _currentUserContext.UserName;
    }
}

public sealed class CreateAdminCustomerAddressCommandHandler : IRequestHandler<CreateAdminCustomerAddressCommand, CustomerAddressResponse>
{
    private readonly IBookingRepository _bookingRepository;
    private readonly IBookingLookupRepository _bookingLookupRepository;
    private readonly ICustomerAppRepository _customerAppRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentDateTime _currentDateTime;
    private readonly ICurrentUserContext _currentUserContext;

    public CreateAdminCustomerAddressCommandHandler(
        IBookingRepository bookingRepository,
        IBookingLookupRepository bookingLookupRepository,
        ICustomerAppRepository customerAppRepository,
        IUnitOfWork unitOfWork,
        ICurrentDateTime currentDateTime,
        ICurrentUserContext currentUserContext)
    {
        _bookingRepository = bookingRepository;
        _bookingLookupRepository = bookingLookupRepository;
        _customerAppRepository = customerAppRepository;
        _unitOfWork = unitOfWork;
        _currentDateTime = currentDateTime;
        _currentUserContext = currentUserContext;
    }

    public async Task<CustomerAddressResponse> Handle(CreateAdminCustomerAddressCommand request, CancellationToken cancellationToken)
    {
        var customer = await _bookingRepository.GetCustomerByIdAsync(request.CustomerId, cancellationToken)
            ?? throw new AppException(ErrorCodes.NotFound, "The requested customer could not be found.", 404);
        var zoneId = await ResolveZoneIdAsync(request.ZoneId, request.Pincode, cancellationToken);
        var now = _currentDateTime.UtcNow;
        var actor = ResolveActor();

        if (request.IsDefault)
        {
            await ClearDefaultAddressesAsync(customer.CustomerId, null, now, actor, cancellationToken);
        }

        var address = new CustomerAddress
        {
            CustomerId = customer.CustomerId,
            ZoneId = zoneId,
            AddressLabel = request.AddressLabel.Trim(),
            AddressLine1 = request.AddressLine1.Trim(),
            AddressLine2 = request.AddressLine2.Trim(),
            Landmark = request.Landmark.Trim(),
            CityName = request.CityName.Trim(),
            StateName = request.StateName?.Trim() ?? string.Empty,
            Pincode = request.Pincode.Trim(),
            AddressType = request.AddressType?.Trim() ?? request.AddressLabel.Trim(),
            Latitude = request.Latitude,
            Longitude = request.Longitude,
            IsDefault = request.IsDefault,
            IsActive = true,
            CreatedBy = actor,
            DateCreated = now,
            IPAddress = ResolveIpAddress()
        };

        await _customerAppRepository.AddAddressAsync(address, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return CustomerAppMapper.ToAddress(address);
    }

    private async Task<long> ResolveZoneIdAsync(long? zoneId, string pincode, CancellationToken cancellationToken)
    {
        if (zoneId.HasValue && zoneId.Value > 0)
        {
            return zoneId.Value;
        }

        var zone = await _bookingLookupRepository.GetZoneByPincodeAsync(pincode.Trim(), cancellationToken)
            ?? throw new AppException(ErrorCodes.ZoneNotServed, "The provided pincode is not serviceable.", 404);

        return zone.ZoneId;
    }

    private async Task ClearDefaultAddressesAsync(long customerId, long? excludedAddressId, DateTime now, string actor, CancellationToken cancellationToken)
    {
        var defaultAddresses = await _customerAppRepository.ListDefaultAddressesForUpdateAsync(customerId, excludedAddressId, cancellationToken);

        foreach (var defaultAddress in defaultAddresses)
        {
            defaultAddress.IsDefault = false;
            defaultAddress.LastUpdated = now;
            defaultAddress.UpdatedBy = actor;
        }
    }

    private string ResolveActor() => string.IsNullOrWhiteSpace(_currentUserContext.UserName) ? "CustomerManagement" : _currentUserContext.UserName;

    private string ResolveIpAddress() => string.IsNullOrWhiteSpace(_currentUserContext.IPAddress) ? "127.0.0.1" : _currentUserContext.IPAddress;
}

public sealed class UpdateAdminCustomerAddressCommandHandler : IRequestHandler<UpdateAdminCustomerAddressCommand, CustomerAddressResponse>
{
    private readonly IBookingRepository _bookingRepository;
    private readonly IBookingLookupRepository _bookingLookupRepository;
    private readonly ICustomerAppRepository _customerAppRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentDateTime _currentDateTime;
    private readonly ICurrentUserContext _currentUserContext;

    public UpdateAdminCustomerAddressCommandHandler(
        IBookingRepository bookingRepository,
        IBookingLookupRepository bookingLookupRepository,
        ICustomerAppRepository customerAppRepository,
        IUnitOfWork unitOfWork,
        ICurrentDateTime currentDateTime,
        ICurrentUserContext currentUserContext)
    {
        _bookingRepository = bookingRepository;
        _bookingLookupRepository = bookingLookupRepository;
        _customerAppRepository = customerAppRepository;
        _unitOfWork = unitOfWork;
        _currentDateTime = currentDateTime;
        _currentUserContext = currentUserContext;
    }

    public async Task<CustomerAddressResponse> Handle(UpdateAdminCustomerAddressCommand request, CancellationToken cancellationToken)
    {
        var customer = await _bookingRepository.GetCustomerByIdAsync(request.CustomerId, cancellationToken)
            ?? throw new AppException(ErrorCodes.NotFound, "The requested customer could not be found.", 404);
        var address = await _customerAppRepository.GetAddressForUpdateAsync(customer.CustomerId, request.AddressId, cancellationToken)
            ?? throw new AppException(ErrorCodes.NotFound, "The customer address could not be found.", 404);
        var zoneId = await ResolveZoneIdAsync(request.ZoneId, request.Pincode, cancellationToken);
        var now = _currentDateTime.UtcNow;
        var actor = ResolveActor();

        if (request.IsDefault)
        {
            await ClearDefaultAddressesAsync(customer.CustomerId, address.CustomerAddressId, now, actor, cancellationToken);
        }

        address.ZoneId = zoneId;
        address.AddressLabel = request.AddressLabel.Trim();
        address.AddressLine1 = request.AddressLine1.Trim();
        address.AddressLine2 = request.AddressLine2.Trim();
        address.Landmark = request.Landmark.Trim();
        address.CityName = request.CityName.Trim();
        address.StateName = request.StateName?.Trim() ?? string.Empty;
        address.Pincode = request.Pincode.Trim();
        address.AddressType = request.AddressType?.Trim() ?? request.AddressLabel.Trim();
        address.Latitude = request.Latitude;
        address.Longitude = request.Longitude;
        address.IsDefault = request.IsDefault;
        address.IsActive = true;
        address.LastUpdated = now;
        address.UpdatedBy = actor;

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return CustomerAppMapper.ToAddress(address);
    }

    private async Task<long> ResolveZoneIdAsync(long? zoneId, string pincode, CancellationToken cancellationToken)
    {
        if (zoneId.HasValue && zoneId.Value > 0)
        {
            return zoneId.Value;
        }

        var zone = await _bookingLookupRepository.GetZoneByPincodeAsync(pincode.Trim(), cancellationToken)
            ?? throw new AppException(ErrorCodes.ZoneNotServed, "The provided pincode is not serviceable.", 404);

        return zone.ZoneId;
    }

    private async Task ClearDefaultAddressesAsync(long customerId, long? excludedAddressId, DateTime now, string actor, CancellationToken cancellationToken)
    {
        var defaultAddresses = await _customerAppRepository.ListDefaultAddressesForUpdateAsync(customerId, excludedAddressId, cancellationToken);

        foreach (var defaultAddress in defaultAddresses)
        {
            defaultAddress.IsDefault = false;
            defaultAddress.LastUpdated = now;
            defaultAddress.UpdatedBy = actor;
        }
    }

    private string ResolveActor() => string.IsNullOrWhiteSpace(_currentUserContext.UserName) ? "CustomerManagement" : _currentUserContext.UserName;
}

public sealed class CreateAdminCustomerEquipmentCommandHandler : IRequestHandler<CreateAdminCustomerEquipmentCommand, CustomerEquipmentResponse>
{
    private readonly IBookingRepository _bookingRepository;
    private readonly ICustomerAppRepository _customerAppRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentDateTime _currentDateTime;
    private readonly ICurrentUserContext _currentUserContext;

    public CreateAdminCustomerEquipmentCommandHandler(
        IBookingRepository bookingRepository,
        ICustomerAppRepository customerAppRepository,
        IUnitOfWork unitOfWork,
        ICurrentDateTime currentDateTime,
        ICurrentUserContext currentUserContext)
    {
        _bookingRepository = bookingRepository;
        _customerAppRepository = customerAppRepository;
        _unitOfWork = unitOfWork;
        _currentDateTime = currentDateTime;
        _currentUserContext = currentUserContext;
    }

    public async Task<CustomerEquipmentResponse> Handle(CreateAdminCustomerEquipmentCommand request, CancellationToken cancellationToken)
    {
        var customer = await _bookingRepository.GetCustomerByIdAsync(request.CustomerId, cancellationToken)
            ?? throw new AppException(ErrorCodes.NotFound, "The requested customer could not be found.", 404);
        var now = _currentDateTime.UtcNow;
        var equipment = new CustomerEquipment
        {
            CustomerId = customer.CustomerId,
            EquipmentName = request.Name.Trim(),
            EquipmentType = request.Type.Trim(),
            BrandName = request.Brand.Trim(),
            Capacity = request.Capacity.Trim(),
            LocationLabel = request.Location.Trim(),
            PurchaseDate = request.PurchaseDate,
            LastServiceDate = request.LastServiceDate,
            SerialNumber = request.SerialNumber?.Trim() ?? string.Empty,
            IsActive = true,
            CreatedBy = ResolveActor(),
            DateCreated = now,
            IPAddress = ResolveIpAddress()
        };

        await _customerAppRepository.AddEquipmentAsync(equipment, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return CustomerAppMapper.ToEquipment(equipment);
    }

    private string ResolveActor() => string.IsNullOrWhiteSpace(_currentUserContext.UserName) ? "CustomerManagement" : _currentUserContext.UserName;

    private string ResolveIpAddress() => string.IsNullOrWhiteSpace(_currentUserContext.IPAddress) ? "127.0.0.1" : _currentUserContext.IPAddress;
}

public sealed class UpdateAdminCustomerEquipmentCommandHandler : IRequestHandler<UpdateAdminCustomerEquipmentCommand, CustomerEquipmentResponse>
{
    private readonly IBookingRepository _bookingRepository;
    private readonly ICustomerAppRepository _customerAppRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentDateTime _currentDateTime;
    private readonly ICurrentUserContext _currentUserContext;

    public UpdateAdminCustomerEquipmentCommandHandler(
        IBookingRepository bookingRepository,
        ICustomerAppRepository customerAppRepository,
        IUnitOfWork unitOfWork,
        ICurrentDateTime currentDateTime,
        ICurrentUserContext currentUserContext)
    {
        _bookingRepository = bookingRepository;
        _customerAppRepository = customerAppRepository;
        _unitOfWork = unitOfWork;
        _currentDateTime = currentDateTime;
        _currentUserContext = currentUserContext;
    }

    public async Task<CustomerEquipmentResponse> Handle(UpdateAdminCustomerEquipmentCommand request, CancellationToken cancellationToken)
    {
        var customer = await _bookingRepository.GetCustomerByIdAsync(request.CustomerId, cancellationToken)
            ?? throw new AppException(ErrorCodes.NotFound, "The requested customer could not be found.", 404);
        var equipment = await _customerAppRepository.GetEquipmentForUpdateAsync(customer.CustomerId, request.CustomerEquipmentId, cancellationToken)
            ?? throw new AppException(ErrorCodes.NotFound, "The customer equipment could not be found.", 404);

        equipment.EquipmentName = request.Name.Trim();
        equipment.EquipmentType = request.Type.Trim();
        equipment.BrandName = request.Brand.Trim();
        equipment.Capacity = request.Capacity.Trim();
        equipment.LocationLabel = request.Location.Trim();
        equipment.PurchaseDate = request.PurchaseDate;
        equipment.LastServiceDate = request.LastServiceDate;
        equipment.SerialNumber = request.SerialNumber?.Trim() ?? string.Empty;
        equipment.IsActive = true;
        equipment.LastUpdated = _currentDateTime.UtcNow;
        equipment.UpdatedBy = ResolveActor();

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return CustomerAppMapper.ToEquipment(equipment);
    }

    private string ResolveActor() => string.IsNullOrWhiteSpace(_currentUserContext.UserName) ? "CustomerManagement" : _currentUserContext.UserName;
}

public sealed class CreateCustomerNoteCommandHandler : IRequestHandler<CreateCustomerNoteCommand, CustomerNoteResponse>
{
    private readonly ICustomerManagementRepository _customerManagementRepository;
    private readonly IAuditLogRepository _auditLogRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentDateTime _currentDateTime;
    private readonly ICurrentUserContext _currentUserContext;

    public CreateCustomerNoteCommandHandler(
        ICustomerManagementRepository customerManagementRepository,
        IAuditLogRepository auditLogRepository,
        IUnitOfWork unitOfWork,
        ICurrentDateTime currentDateTime,
        ICurrentUserContext currentUserContext)
    {
        _customerManagementRepository = customerManagementRepository;
        _auditLogRepository = auditLogRepository;
        _unitOfWork = unitOfWork;
        _currentDateTime = currentDateTime;
        _currentUserContext = currentUserContext;
    }

    public async Task<CustomerNoteResponse> Handle(CreateCustomerNoteCommand request, CancellationToken cancellationToken)
    {
        var customer = await _customerManagementRepository.GetCustomerForUpdateAsync(request.CustomerId, cancellationToken)
            ?? throw new AppException(ErrorCodes.NotFound, "The requested customer could not be found.", 404);

        var auditLog = new AuditLog
        {
            UserId = _currentUserContext.UserId,
            ActionName = "CustomerNoteAdded",
            EntityName = "Customer",
            EntityId = customer.CustomerId.ToString(),
            TraceId = _currentUserContext.TraceId ?? string.Empty,
            StatusName = request.IsPrivate ? "Private" : "Public",
            NewValues = request.Content.Trim(),
            Comments = string.IsNullOrWhiteSpace(request.NoteType) ? "Internal" : request.NoteType.Trim(),
            CreatedBy = ResolveActor(),
            DateCreated = _currentDateTime.UtcNow,
            IPAddress = ResolveIpAddress()
        };

        await _auditLogRepository.AddAsync(auditLog, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return CustomerManagementMapper.ToNoteResponse(auditLog);
    }

    private string ResolveActor() => string.IsNullOrWhiteSpace(_currentUserContext.UserName) ? "CustomerManagement" : _currentUserContext.UserName;

    private string ResolveIpAddress() => string.IsNullOrWhiteSpace(_currentUserContext.IPAddress) ? "127.0.0.1" : _currentUserContext.IPAddress;
}
