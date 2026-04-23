using Coolzo.Application.Common.Interfaces;
using Coolzo.Application.Common.Security;
using Coolzo.Application.Features.Booking;
using Coolzo.Contracts.Common;
using Coolzo.Contracts.Responses.Booking;
using Coolzo.Contracts.Responses.CMS;
using Coolzo.Contracts.Responses.Customer;
using Coolzo.Domain.Entities;
using Coolzo.Domain.Enums;
using Coolzo.Shared.Constants;
using Coolzo.Shared.Exceptions;
using Coolzo.Shared.Models;
using MediatR;

namespace Coolzo.Application.Features.CustomerApp;

public sealed record GetMyCustomerProfileQuery : IRequest<CustomerProfileResponse>;

public sealed record UpdateMyCustomerProfileCommand(
    string CustomerName,
    string MobileNumber,
    string EmailAddress,
    string? PhotoUrl,
    string? MembershipStatus) : IRequest<CustomerProfileResponse>;

public sealed record DeactivateMyCustomerAccountCommand(string? Reason) : IRequest<CustomerAccountDeletionResponse>;

public sealed record GetMyCustomerAddressesQuery : IRequest<IReadOnlyCollection<CustomerAddressResponse>>;

public sealed record CreateMyCustomerAddressCommand(
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

public sealed record UpdateMyCustomerAddressCommand(
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

public sealed record DeleteMyCustomerAddressCommand(long AddressId) : IRequest<Unit>;

public sealed record GetMyCustomerEquipmentQuery : IRequest<IReadOnlyCollection<CustomerEquipmentResponse>>;

public sealed record CreateMyCustomerEquipmentCommand(
    string Name,
    string Type,
    string Brand,
    string Capacity,
    string Location,
    DateOnly? PurchaseDate,
    DateOnly? LastServiceDate,
    string? SerialNumber) : IRequest<CustomerEquipmentResponse>;

public sealed record UpdateMyCustomerEquipmentCommand(
    long CustomerEquipmentId,
    string Name,
    string Type,
    string Brand,
    string Capacity,
    string Location,
    DateOnly? PurchaseDate,
    DateOnly? LastServiceDate,
    string? SerialNumber) : IRequest<CustomerEquipmentResponse>;

public sealed record DeleteMyCustomerEquipmentCommand(long CustomerEquipmentId) : IRequest<Unit>;

public sealed record GetMyNotificationsQuery(int PageNumber, int PageSize) : IRequest<PagedResult<CustomerNotificationResponse>>;

public sealed record MarkMyNotificationReadCommand(long CustomerNotificationId) : IRequest<Unit>;

public sealed record GetActivePromotionalOffersQuery : IRequest<IReadOnlyCollection<PromotionalOfferResponse>>;

public sealed record ValidateCouponQuery(string Code) : IRequest<PromotionalOfferResponse?>;

public sealed record GetMyReferralStatsQuery : IRequest<ReferralStatsResponse>;

public sealed record GetMyLoyaltyPointsQuery : IRequest<LoyaltyPointsResponse>;

public sealed record GetMyLoyaltyTransactionsQuery : IRequest<IReadOnlyCollection<LoyaltyTransactionResponse>>;

public sealed record GetCustomerReviewsQuery(long? ServiceId) : IRequest<IReadOnlyCollection<CustomerReviewResponse>>;

public sealed record CreateMyCustomerReviewCommand(
    int Rating,
    string Comment,
    long? BookingId,
    long? ServiceId,
    string? CustomerPhotoUrl) : IRequest<CustomerReviewResponse>;

public sealed record SubmitMyAppFeedbackCommand(
    string? FeedbackType,
    string Message,
    int? Rating,
    string? AppVersion,
    string? DeviceInfo) : IRequest<CustomerAppFeedbackResponse>;

public sealed record RescheduleMyCustomerBookingCommand(
    long BookingId,
    long SlotAvailabilityId,
    string? Remarks) : IRequest<BookingDetailResponse>;

public sealed record GetCustomerVisibleTechnicianQuery(long TechnicianId) : IRequest<CustomerVisibleTechnicianResponse>;

public sealed record GetPublicBlogsQuery : IRequest<IReadOnlyCollection<BlogContentResponse>>;

public sealed record GetPublicBlogByIdQuery(string Id) : IRequest<BlogContentResponse?>;

public sealed record GetPublicChangelogQuery : IRequest<IReadOnlyCollection<ChangelogItemResponse>>;

internal static class CustomerAppMapper
{
    public static CustomerProfileResponse ToProfile(Customer customer)
    {
        return new CustomerProfileResponse(
            customer.CustomerId,
            customer.UserId,
            customer.CustomerName,
            customer.MobileNumber,
            customer.EmailAddress,
            customer.Tag ?? string.Empty,
            customer.Comments ?? "none",
            customer.IsActive,
            customer.DateCreated,
            customer.LastUpdated);
    }

    public static CustomerAddressResponse ToAddress(CustomerAddress address)
    {
        return new CustomerAddressResponse(
            address.CustomerAddressId,
            address.CustomerId,
            address.AddressLabel,
            address.AddressLine1,
            address.AddressLine2,
            address.Landmark,
            address.CityName,
            address.StateName,
            address.Pincode,
            address.AddressType,
            address.ZoneId,
            address.Latitude,
            address.Longitude,
            address.IsDefault,
            address.IsActive,
            address.DateCreated,
            address.LastUpdated);
    }

    public static CustomerEquipmentResponse ToEquipment(CustomerEquipment equipment)
    {
        return new CustomerEquipmentResponse(
            equipment.CustomerEquipmentId,
            equipment.CustomerId,
            equipment.EquipmentName,
            equipment.EquipmentType,
            equipment.BrandName,
            equipment.Capacity,
            equipment.LocationLabel,
            equipment.PurchaseDate,
            equipment.LastServiceDate,
            equipment.SerialNumber,
            equipment.IsActive,
            equipment.DateCreated,
            equipment.LastUpdated);
    }

    public static CustomerNotificationResponse ToNotification(CustomerNotification notification)
    {
        return new CustomerNotificationResponse(
            notification.CustomerNotificationId,
            notification.CustomerId,
            notification.Title,
            notification.Message,
            notification.NotificationType,
            notification.IsRead,
            notification.DateCreated,
            notification.LinkUrl);
    }

    public static PromotionalOfferResponse ToOffer(PromotionalOffer offer)
    {
        return new PromotionalOfferResponse(
            offer.PromotionalOfferId,
            offer.OfferCode,
            offer.Title,
            offer.Description,
            offer.DiscountType,
            offer.DiscountValue,
            offer.MinimumOrderValue,
            offer.ExpiryDate,
            offer.Category);
    }

    public static ReferralResponse ToReferral(CustomerReferral referral)
    {
        return new ReferralResponse(
            referral.CustomerReferralId,
            referral.ReferralName,
            referral.ReferralStatus,
            referral.RewardAmount,
            referral.ReferralDate);
    }

    public static LoyaltyTransactionResponse ToLoyaltyTransaction(CustomerLoyaltyTransaction transaction)
    {
        return new LoyaltyTransactionResponse(
            transaction.CustomerLoyaltyTransactionId,
            transaction.TransactionType,
            transaction.Points,
            transaction.Description,
            transaction.DateCreated);
    }

    public static CustomerReviewResponse ToReview(CustomerReview review)
    {
        return new CustomerReviewResponse(
            review.CustomerReviewId,
            review.CustomerId,
            review.CustomerNameSnapshot,
            review.CustomerPhotoUrl,
            review.Rating,
            review.Comment,
            review.BookingId,
            review.ServiceId,
            review.DateCreated);
    }

    public static BlogContentResponse ToBlog(CMSBlock block)
    {
        return new BlogContentResponse(
            block.BlockKey,
            block.Title,
            block.Summary,
            block.Content,
            block.CreatedBy,
            block.DatePublished ?? block.DateCreated,
            block.PreviewImageUrl,
            ResolveBlogCategory(block.BlockKey));
    }

    public static ChangelogItemResponse ToChangelog(CMSBlock block)
    {
        var changes = block.Content
            .Split(new[] { '\r', '\n', ';' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(change => change.TrimStart('-', '*', ' '))
            .Where(change => change.Length > 0)
            .ToArray();

        return new ChangelogItemResponse(
            block.Title,
            block.DatePublished ?? block.DateCreated,
            changes);
    }

    private static string ResolveBlogCategory(string blockKey)
    {
        var parts = blockKey.Split(':', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        return parts.Length >= 3 ? parts[1] : "Blog";
    }
}

internal static class CustomerAppAccess
{
    public static async Task<CustomerAccountLookupResult> ResolveCurrentCustomerAsync(
        ICurrentUserContext currentUserContext,
        CustomerAccountLookupService customerAccountLookupService,
        CancellationToken cancellationToken)
    {
        if (!currentUserContext.IsAuthenticated || !currentUserContext.UserId.HasValue)
        {
            throw new AppException(ErrorCodes.Unauthorized, "Customer authentication is required.", 401);
        }

        return await customerAccountLookupService.FindByUserIdAsync(currentUserContext.UserId.Value, cancellationToken)
            ?? throw new AppException(ErrorCodes.Forbidden, "The current customer could not be resolved.", 403);
    }

    public static string ResolveActor(ICurrentUserContext currentUserContext)
    {
        return string.IsNullOrWhiteSpace(currentUserContext.UserName)
            ? "CustomerApp"
            : currentUserContext.UserName;
    }

    public static string ResolveIpAddress(ICurrentUserContext currentUserContext)
    {
        return string.IsNullOrWhiteSpace(currentUserContext.IPAddress)
            ? "127.0.0.1"
            : currentUserContext.IPAddress;
    }
}

public sealed class GetMyCustomerProfileQueryHandler : IRequestHandler<GetMyCustomerProfileQuery, CustomerProfileResponse>
{
    private readonly CustomerAccountLookupService _customerAccountLookupService;
    private readonly ICurrentUserContext _currentUserContext;

    public GetMyCustomerProfileQueryHandler(CustomerAccountLookupService customerAccountLookupService, ICurrentUserContext currentUserContext)
    {
        _customerAccountLookupService = customerAccountLookupService;
        _currentUserContext = currentUserContext;
    }

    public async Task<CustomerProfileResponse> Handle(GetMyCustomerProfileQuery request, CancellationToken cancellationToken)
    {
        var account = await CustomerAppAccess.ResolveCurrentCustomerAsync(_currentUserContext, _customerAccountLookupService, cancellationToken);
        return CustomerAppMapper.ToProfile(account.Customer);
    }
}

public sealed class UpdateMyCustomerProfileCommandHandler : IRequestHandler<UpdateMyCustomerProfileCommand, CustomerProfileResponse>
{
    private readonly CustomerAccountLookupService _customerAccountLookupService;
    private readonly ICurrentDateTime _currentDateTime;
    private readonly ICurrentUserContext _currentUserContext;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUserRepository _userRepository;

    public UpdateMyCustomerProfileCommandHandler(
        CustomerAccountLookupService customerAccountLookupService,
        IUserRepository userRepository,
        IUnitOfWork unitOfWork,
        ICurrentDateTime currentDateTime,
        ICurrentUserContext currentUserContext)
    {
        _customerAccountLookupService = customerAccountLookupService;
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
        _currentDateTime = currentDateTime;
        _currentUserContext = currentUserContext;
    }

    public async Task<CustomerProfileResponse> Handle(UpdateMyCustomerProfileCommand request, CancellationToken cancellationToken)
    {
        var account = await CustomerAppAccess.ResolveCurrentCustomerAsync(_currentUserContext, _customerAccountLookupService, cancellationToken);
        var normalizedName = request.CustomerName.Trim();
        var normalizedMobile = request.MobileNumber.Trim();
        var normalizedEmail = request.EmailAddress.Trim();

        if (await _userRepository.ExistsByUserNameAsync(normalizedMobile, account.User.UserId, cancellationToken))
        {
            throw new AppException(ErrorCodes.DuplicateValue, "The mobile number is already assigned to another login.", 409);
        }

        if (await _userRepository.ExistsByEmailAsync(normalizedEmail, account.User.UserId, cancellationToken))
        {
            throw new AppException(ErrorCodes.DuplicateValue, "The email address is already assigned to another login.", 409);
        }

        var actor = CustomerAppAccess.ResolveActor(_currentUserContext);
        var now = _currentDateTime.UtcNow;
        account.Customer.CustomerName = normalizedName;
        account.Customer.MobileNumber = normalizedMobile;
        account.Customer.EmailAddress = normalizedEmail;
        account.Customer.Tag = request.PhotoUrl?.Trim() ?? string.Empty;
        account.Customer.Comments = request.MembershipStatus?.Trim() ?? "none";
        account.Customer.LastUpdated = now;
        account.Customer.UpdatedBy = actor;

        account.User.FullName = normalizedName;
        account.User.UserName = normalizedMobile;
        account.User.Email = normalizedEmail;
        account.User.LastUpdated = now;
        account.User.UpdatedBy = actor;

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return CustomerAppMapper.ToProfile(account.Customer);
    }
}

public sealed class DeactivateMyCustomerAccountCommandHandler : IRequestHandler<DeactivateMyCustomerAccountCommand, CustomerAccountDeletionResponse>
{
    private readonly CustomerAccountLookupService _customerAccountLookupService;
    private readonly ICurrentDateTime _currentDateTime;
    private readonly ICurrentUserContext _currentUserContext;
    private readonly IUnitOfWork _unitOfWork;

    public DeactivateMyCustomerAccountCommandHandler(
        CustomerAccountLookupService customerAccountLookupService,
        IUnitOfWork unitOfWork,
        ICurrentDateTime currentDateTime,
        ICurrentUserContext currentUserContext)
    {
        _customerAccountLookupService = customerAccountLookupService;
        _unitOfWork = unitOfWork;
        _currentDateTime = currentDateTime;
        _currentUserContext = currentUserContext;
    }

    public async Task<CustomerAccountDeletionResponse> Handle(DeactivateMyCustomerAccountCommand request, CancellationToken cancellationToken)
    {
        var account = await CustomerAppAccess.ResolveCurrentCustomerAsync(_currentUserContext, _customerAccountLookupService, cancellationToken);
        var now = _currentDateTime.UtcNow;
        var actor = CustomerAppAccess.ResolveActor(_currentUserContext);
        var reason = string.IsNullOrWhiteSpace(request.Reason) ? "Customer requested account deactivation." : request.Reason.Trim();

        account.Customer.IsActive = false;
        account.Customer.LastUpdated = now;
        account.Customer.UpdatedBy = actor;
        account.Customer.Comments = reason;
        account.User.IsActive = false;
        account.User.LastUpdated = now;
        account.User.UpdatedBy = actor;

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return new CustomerAccountDeletionResponse(account.Customer.CustomerId, account.Customer.IsActive, now, reason);
    }
}

public sealed class GetMyCustomerAddressesQueryHandler : IRequestHandler<GetMyCustomerAddressesQuery, IReadOnlyCollection<CustomerAddressResponse>>
{
    private readonly CustomerAccountLookupService _customerAccountLookupService;
    private readonly ICustomerAppRepository _customerAppRepository;
    private readonly ICurrentUserContext _currentUserContext;

    public GetMyCustomerAddressesQueryHandler(
        CustomerAccountLookupService customerAccountLookupService,
        ICustomerAppRepository customerAppRepository,
        ICurrentUserContext currentUserContext)
    {
        _customerAccountLookupService = customerAccountLookupService;
        _customerAppRepository = customerAppRepository;
        _currentUserContext = currentUserContext;
    }

    public async Task<IReadOnlyCollection<CustomerAddressResponse>> Handle(GetMyCustomerAddressesQuery request, CancellationToken cancellationToken)
    {
        var account = await CustomerAppAccess.ResolveCurrentCustomerAsync(_currentUserContext, _customerAccountLookupService, cancellationToken);
        var addresses = await _customerAppRepository.ListAddressesAsync(account.Customer.CustomerId, cancellationToken);
        return addresses.Select(CustomerAppMapper.ToAddress).ToArray();
    }
}

public sealed class CreateMyCustomerAddressCommandHandler : IRequestHandler<CreateMyCustomerAddressCommand, CustomerAddressResponse>
{
    private readonly IBookingLookupRepository _bookingLookupRepository;
    private readonly CustomerAccountLookupService _customerAccountLookupService;
    private readonly ICustomerAppRepository _customerAppRepository;
    private readonly ICurrentDateTime _currentDateTime;
    private readonly ICurrentUserContext _currentUserContext;
    private readonly IUnitOfWork _unitOfWork;

    public CreateMyCustomerAddressCommandHandler(
        CustomerAccountLookupService customerAccountLookupService,
        ICustomerAppRepository customerAppRepository,
        IBookingLookupRepository bookingLookupRepository,
        IUnitOfWork unitOfWork,
        ICurrentDateTime currentDateTime,
        ICurrentUserContext currentUserContext)
    {
        _customerAccountLookupService = customerAccountLookupService;
        _customerAppRepository = customerAppRepository;
        _bookingLookupRepository = bookingLookupRepository;
        _unitOfWork = unitOfWork;
        _currentDateTime = currentDateTime;
        _currentUserContext = currentUserContext;
    }

    public async Task<CustomerAddressResponse> Handle(CreateMyCustomerAddressCommand request, CancellationToken cancellationToken)
    {
        var account = await CustomerAppAccess.ResolveCurrentCustomerAsync(_currentUserContext, _customerAccountLookupService, cancellationToken);
        var zoneId = await ResolveZoneIdAsync(request.ZoneId, request.Pincode, cancellationToken);
        var now = _currentDateTime.UtcNow;
        var actor = CustomerAppAccess.ResolveActor(_currentUserContext);
        var address = new CustomerAddress
        {
            CustomerId = account.Customer.CustomerId,
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
            IPAddress = CustomerAppAccess.ResolveIpAddress(_currentUserContext)
        };

        if (address.IsDefault)
        {
            await ClearDefaultAddressesAsync(account.Customer.CustomerId, null, now, actor, cancellationToken);
        }

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
}

public sealed class UpdateMyCustomerAddressCommandHandler : IRequestHandler<UpdateMyCustomerAddressCommand, CustomerAddressResponse>
{
    private readonly IBookingLookupRepository _bookingLookupRepository;
    private readonly CustomerAccountLookupService _customerAccountLookupService;
    private readonly ICustomerAppRepository _customerAppRepository;
    private readonly ICurrentDateTime _currentDateTime;
    private readonly ICurrentUserContext _currentUserContext;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateMyCustomerAddressCommandHandler(
        CustomerAccountLookupService customerAccountLookupService,
        ICustomerAppRepository customerAppRepository,
        IBookingLookupRepository bookingLookupRepository,
        IUnitOfWork unitOfWork,
        ICurrentDateTime currentDateTime,
        ICurrentUserContext currentUserContext)
    {
        _customerAccountLookupService = customerAccountLookupService;
        _customerAppRepository = customerAppRepository;
        _bookingLookupRepository = bookingLookupRepository;
        _unitOfWork = unitOfWork;
        _currentDateTime = currentDateTime;
        _currentUserContext = currentUserContext;
    }

    public async Task<CustomerAddressResponse> Handle(UpdateMyCustomerAddressCommand request, CancellationToken cancellationToken)
    {
        var account = await CustomerAppAccess.ResolveCurrentCustomerAsync(_currentUserContext, _customerAccountLookupService, cancellationToken);
        var address = await _customerAppRepository.GetAddressForUpdateAsync(account.Customer.CustomerId, request.AddressId, cancellationToken)
            ?? throw new AppException(ErrorCodes.NotFound, "The customer address could not be found.", 404);
        var zoneId = await ResolveZoneIdAsync(request.ZoneId, request.Pincode, cancellationToken);
        var now = _currentDateTime.UtcNow;
        var actor = CustomerAppAccess.ResolveActor(_currentUserContext);

        if (request.IsDefault)
        {
            await ClearDefaultAddressesAsync(account.Customer.CustomerId, address.CustomerAddressId, now, actor, cancellationToken);
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
}

public sealed class DeleteMyCustomerAddressCommandHandler : IRequestHandler<DeleteMyCustomerAddressCommand, Unit>
{
    private readonly CustomerAccountLookupService _customerAccountLookupService;
    private readonly ICustomerAppRepository _customerAppRepository;
    private readonly ICurrentDateTime _currentDateTime;
    private readonly ICurrentUserContext _currentUserContext;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteMyCustomerAddressCommandHandler(
        CustomerAccountLookupService customerAccountLookupService,
        ICustomerAppRepository customerAppRepository,
        IUnitOfWork unitOfWork,
        ICurrentDateTime currentDateTime,
        ICurrentUserContext currentUserContext)
    {
        _customerAccountLookupService = customerAccountLookupService;
        _customerAppRepository = customerAppRepository;
        _unitOfWork = unitOfWork;
        _currentDateTime = currentDateTime;
        _currentUserContext = currentUserContext;
    }

    public async Task<Unit> Handle(DeleteMyCustomerAddressCommand request, CancellationToken cancellationToken)
    {
        var account = await CustomerAppAccess.ResolveCurrentCustomerAsync(_currentUserContext, _customerAccountLookupService, cancellationToken);
        var address = await _customerAppRepository.GetAddressForUpdateAsync(account.Customer.CustomerId, request.AddressId, cancellationToken)
            ?? throw new AppException(ErrorCodes.NotFound, "The customer address could not be found.", 404);
        address.IsActive = false;
        address.IsDefault = false;
        address.IsDeleted = true;
        address.DateDeleted = _currentDateTime.UtcNow;
        address.DeletedBy = CustomerAppAccess.ResolveActor(_currentUserContext);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}

public sealed class GetMyCustomerEquipmentQueryHandler : IRequestHandler<GetMyCustomerEquipmentQuery, IReadOnlyCollection<CustomerEquipmentResponse>>
{
    private readonly CustomerAccountLookupService _customerAccountLookupService;
    private readonly ICustomerAppRepository _customerAppRepository;
    private readonly ICurrentUserContext _currentUserContext;

    public GetMyCustomerEquipmentQueryHandler(
        CustomerAccountLookupService customerAccountLookupService,
        ICustomerAppRepository customerAppRepository,
        ICurrentUserContext currentUserContext)
    {
        _customerAccountLookupService = customerAccountLookupService;
        _customerAppRepository = customerAppRepository;
        _currentUserContext = currentUserContext;
    }

    public async Task<IReadOnlyCollection<CustomerEquipmentResponse>> Handle(GetMyCustomerEquipmentQuery request, CancellationToken cancellationToken)
    {
        var account = await CustomerAppAccess.ResolveCurrentCustomerAsync(_currentUserContext, _customerAccountLookupService, cancellationToken);
        var equipment = await _customerAppRepository.ListEquipmentAsync(account.Customer.CustomerId, cancellationToken);
        return equipment.Select(CustomerAppMapper.ToEquipment).ToArray();
    }
}

public sealed class CreateMyCustomerEquipmentCommandHandler : IRequestHandler<CreateMyCustomerEquipmentCommand, CustomerEquipmentResponse>
{
    private readonly CustomerAccountLookupService _customerAccountLookupService;
    private readonly ICustomerAppRepository _customerAppRepository;
    private readonly ICurrentDateTime _currentDateTime;
    private readonly ICurrentUserContext _currentUserContext;
    private readonly IUnitOfWork _unitOfWork;

    public CreateMyCustomerEquipmentCommandHandler(
        CustomerAccountLookupService customerAccountLookupService,
        ICustomerAppRepository customerAppRepository,
        IUnitOfWork unitOfWork,
        ICurrentDateTime currentDateTime,
        ICurrentUserContext currentUserContext)
    {
        _customerAccountLookupService = customerAccountLookupService;
        _customerAppRepository = customerAppRepository;
        _unitOfWork = unitOfWork;
        _currentDateTime = currentDateTime;
        _currentUserContext = currentUserContext;
    }

    public async Task<CustomerEquipmentResponse> Handle(CreateMyCustomerEquipmentCommand request, CancellationToken cancellationToken)
    {
        var account = await CustomerAppAccess.ResolveCurrentCustomerAsync(_currentUserContext, _customerAccountLookupService, cancellationToken);
        var now = _currentDateTime.UtcNow;
        var equipment = new CustomerEquipment
        {
            CustomerId = account.Customer.CustomerId,
            EquipmentName = request.Name.Trim(),
            EquipmentType = request.Type.Trim(),
            BrandName = request.Brand.Trim(),
            Capacity = request.Capacity.Trim(),
            LocationLabel = request.Location.Trim(),
            PurchaseDate = request.PurchaseDate,
            LastServiceDate = request.LastServiceDate,
            SerialNumber = request.SerialNumber?.Trim() ?? string.Empty,
            IsActive = true,
            CreatedBy = CustomerAppAccess.ResolveActor(_currentUserContext),
            DateCreated = now,
            IPAddress = CustomerAppAccess.ResolveIpAddress(_currentUserContext)
        };

        await _customerAppRepository.AddEquipmentAsync(equipment, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return CustomerAppMapper.ToEquipment(equipment);
    }
}

public sealed class UpdateMyCustomerEquipmentCommandHandler : IRequestHandler<UpdateMyCustomerEquipmentCommand, CustomerEquipmentResponse>
{
    private readonly CustomerAccountLookupService _customerAccountLookupService;
    private readonly ICustomerAppRepository _customerAppRepository;
    private readonly ICurrentDateTime _currentDateTime;
    private readonly ICurrentUserContext _currentUserContext;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateMyCustomerEquipmentCommandHandler(
        CustomerAccountLookupService customerAccountLookupService,
        ICustomerAppRepository customerAppRepository,
        IUnitOfWork unitOfWork,
        ICurrentDateTime currentDateTime,
        ICurrentUserContext currentUserContext)
    {
        _customerAccountLookupService = customerAccountLookupService;
        _customerAppRepository = customerAppRepository;
        _unitOfWork = unitOfWork;
        _currentDateTime = currentDateTime;
        _currentUserContext = currentUserContext;
    }

    public async Task<CustomerEquipmentResponse> Handle(UpdateMyCustomerEquipmentCommand request, CancellationToken cancellationToken)
    {
        var account = await CustomerAppAccess.ResolveCurrentCustomerAsync(_currentUserContext, _customerAccountLookupService, cancellationToken);
        var equipment = await _customerAppRepository.GetEquipmentForUpdateAsync(account.Customer.CustomerId, request.CustomerEquipmentId, cancellationToken)
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
        equipment.UpdatedBy = CustomerAppAccess.ResolveActor(_currentUserContext);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return CustomerAppMapper.ToEquipment(equipment);
    }
}

public sealed class DeleteMyCustomerEquipmentCommandHandler : IRequestHandler<DeleteMyCustomerEquipmentCommand, Unit>
{
    private readonly CustomerAccountLookupService _customerAccountLookupService;
    private readonly ICustomerAppRepository _customerAppRepository;
    private readonly ICurrentDateTime _currentDateTime;
    private readonly ICurrentUserContext _currentUserContext;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteMyCustomerEquipmentCommandHandler(
        CustomerAccountLookupService customerAccountLookupService,
        ICustomerAppRepository customerAppRepository,
        IUnitOfWork unitOfWork,
        ICurrentDateTime currentDateTime,
        ICurrentUserContext currentUserContext)
    {
        _customerAccountLookupService = customerAccountLookupService;
        _customerAppRepository = customerAppRepository;
        _unitOfWork = unitOfWork;
        _currentDateTime = currentDateTime;
        _currentUserContext = currentUserContext;
    }

    public async Task<Unit> Handle(DeleteMyCustomerEquipmentCommand request, CancellationToken cancellationToken)
    {
        var account = await CustomerAppAccess.ResolveCurrentCustomerAsync(_currentUserContext, _customerAccountLookupService, cancellationToken);
        var equipment = await _customerAppRepository.GetEquipmentForUpdateAsync(account.Customer.CustomerId, request.CustomerEquipmentId, cancellationToken)
            ?? throw new AppException(ErrorCodes.NotFound, "The customer equipment could not be found.", 404);
        equipment.IsActive = false;
        equipment.IsDeleted = true;
        equipment.DateDeleted = _currentDateTime.UtcNow;
        equipment.DeletedBy = CustomerAppAccess.ResolveActor(_currentUserContext);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}

public sealed class GetMyNotificationsQueryHandler : IRequestHandler<GetMyNotificationsQuery, PagedResult<CustomerNotificationResponse>>
{
    private readonly CustomerAccountLookupService _customerAccountLookupService;
    private readonly ICustomerAppRepository _customerAppRepository;
    private readonly ICurrentUserContext _currentUserContext;

    public GetMyNotificationsQueryHandler(
        CustomerAccountLookupService customerAccountLookupService,
        ICustomerAppRepository customerAppRepository,
        ICurrentUserContext currentUserContext)
    {
        _customerAccountLookupService = customerAccountLookupService;
        _customerAppRepository = customerAppRepository;
        _currentUserContext = currentUserContext;
    }

    public async Task<PagedResult<CustomerNotificationResponse>> Handle(GetMyNotificationsQuery request, CancellationToken cancellationToken)
    {
        var pageNumber = Math.Max(request.PageNumber, 1);
        var pageSize = Math.Clamp(request.PageSize, 1, 100);
        var account = await CustomerAppAccess.ResolveCurrentCustomerAsync(_currentUserContext, _customerAccountLookupService, cancellationToken);
        var notifications = await _customerAppRepository.ListNotificationsAsync(account.Customer.CustomerId, pageNumber, pageSize, cancellationToken);
        var totalCount = await _customerAppRepository.CountNotificationsAsync(account.Customer.CustomerId, cancellationToken);

        return new PagedResult<CustomerNotificationResponse>(
            notifications.Select(CustomerAppMapper.ToNotification).ToArray(),
            totalCount,
            pageNumber,
            pageSize);
    }
}

public sealed class MarkMyNotificationReadCommandHandler : IRequestHandler<MarkMyNotificationReadCommand, Unit>
{
    private readonly CustomerAccountLookupService _customerAccountLookupService;
    private readonly ICustomerAppRepository _customerAppRepository;
    private readonly ICurrentDateTime _currentDateTime;
    private readonly ICurrentUserContext _currentUserContext;
    private readonly IUnitOfWork _unitOfWork;

    public MarkMyNotificationReadCommandHandler(
        CustomerAccountLookupService customerAccountLookupService,
        ICustomerAppRepository customerAppRepository,
        IUnitOfWork unitOfWork,
        ICurrentDateTime currentDateTime,
        ICurrentUserContext currentUserContext)
    {
        _customerAccountLookupService = customerAccountLookupService;
        _customerAppRepository = customerAppRepository;
        _unitOfWork = unitOfWork;
        _currentDateTime = currentDateTime;
        _currentUserContext = currentUserContext;
    }

    public async Task<Unit> Handle(MarkMyNotificationReadCommand request, CancellationToken cancellationToken)
    {
        var account = await CustomerAppAccess.ResolveCurrentCustomerAsync(_currentUserContext, _customerAccountLookupService, cancellationToken);
        var notification = await _customerAppRepository.GetNotificationForUpdateAsync(account.Customer.CustomerId, request.CustomerNotificationId, cancellationToken)
            ?? throw new AppException(ErrorCodes.NotFound, "The notification could not be found.", 404);
        notification.IsRead = true;
        notification.LastUpdated = _currentDateTime.UtcNow;
        notification.UpdatedBy = CustomerAppAccess.ResolveActor(_currentUserContext);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}

public sealed class GetActivePromotionalOffersQueryHandler : IRequestHandler<GetActivePromotionalOffersQuery, IReadOnlyCollection<PromotionalOfferResponse>>
{
    private readonly ICurrentDateTime _currentDateTime;
    private readonly ICustomerAppRepository _customerAppRepository;

    public GetActivePromotionalOffersQueryHandler(ICustomerAppRepository customerAppRepository, ICurrentDateTime currentDateTime)
    {
        _customerAppRepository = customerAppRepository;
        _currentDateTime = currentDateTime;
    }

    public async Task<IReadOnlyCollection<PromotionalOfferResponse>> Handle(GetActivePromotionalOffersQuery request, CancellationToken cancellationToken)
    {
        var offers = await _customerAppRepository.ListActiveOffersAsync(DateOnly.FromDateTime(_currentDateTime.UtcNow), cancellationToken);
        return offers.Select(CustomerAppMapper.ToOffer).ToArray();
    }
}

public sealed class ValidateCouponQueryHandler : IRequestHandler<ValidateCouponQuery, PromotionalOfferResponse?>
{
    private readonly ICurrentDateTime _currentDateTime;
    private readonly ICustomerAppRepository _customerAppRepository;

    public ValidateCouponQueryHandler(ICustomerAppRepository customerAppRepository, ICurrentDateTime currentDateTime)
    {
        _customerAppRepository = customerAppRepository;
        _currentDateTime = currentDateTime;
    }

    public async Task<PromotionalOfferResponse?> Handle(ValidateCouponQuery request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Code))
        {
            return null;
        }

        var offer = await _customerAppRepository.GetActiveOfferByCodeAsync(
            request.Code.Trim().ToUpperInvariant(),
            DateOnly.FromDateTime(_currentDateTime.UtcNow),
            cancellationToken);

        return offer is null ? null : CustomerAppMapper.ToOffer(offer);
    }
}

public sealed class GetMyReferralStatsQueryHandler : IRequestHandler<GetMyReferralStatsQuery, ReferralStatsResponse>
{
    private readonly CustomerAccountLookupService _customerAccountLookupService;
    private readonly ICustomerAppRepository _customerAppRepository;
    private readonly ICurrentUserContext _currentUserContext;

    public GetMyReferralStatsQueryHandler(
        CustomerAccountLookupService customerAccountLookupService,
        ICustomerAppRepository customerAppRepository,
        ICurrentUserContext currentUserContext)
    {
        _customerAccountLookupService = customerAccountLookupService;
        _customerAppRepository = customerAppRepository;
        _currentUserContext = currentUserContext;
    }

    public async Task<ReferralStatsResponse> Handle(GetMyReferralStatsQuery request, CancellationToken cancellationToken)
    {
        var account = await CustomerAppAccess.ResolveCurrentCustomerAsync(_currentUserContext, _customerAccountLookupService, cancellationToken);
        var referrals = await _customerAppRepository.ListReferralsAsync(account.Customer.CustomerId, cancellationToken);
        var responses = referrals.Select(CustomerAppMapper.ToReferral).ToArray();
        return new ReferralStatsResponse(
            $"COOL{account.Customer.CustomerId:00000}",
            responses.Length,
            responses.Where(referral => referral.Status.Equals("Completed", StringComparison.OrdinalIgnoreCase)).Sum(referral => referral.Reward),
            responses.Count(referral => referral.Status.Equals("Pending", StringComparison.OrdinalIgnoreCase)),
            responses);
    }
}

public sealed class GetMyLoyaltyPointsQueryHandler : IRequestHandler<GetMyLoyaltyPointsQuery, LoyaltyPointsResponse>
{
    private readonly CustomerAccountLookupService _customerAccountLookupService;
    private readonly ICustomerAppRepository _customerAppRepository;
    private readonly ICurrentUserContext _currentUserContext;

    public GetMyLoyaltyPointsQueryHandler(
        CustomerAccountLookupService customerAccountLookupService,
        ICustomerAppRepository customerAppRepository,
        ICurrentUserContext currentUserContext)
    {
        _customerAccountLookupService = customerAccountLookupService;
        _customerAppRepository = customerAppRepository;
        _currentUserContext = currentUserContext;
    }

    public async Task<LoyaltyPointsResponse> Handle(GetMyLoyaltyPointsQuery request, CancellationToken cancellationToken)
    {
        var account = await CustomerAppAccess.ResolveCurrentCustomerAsync(_currentUserContext, _customerAccountLookupService, cancellationToken);
        var transactions = await _customerAppRepository.ListLoyaltyTransactionsAsync(account.Customer.CustomerId, cancellationToken);
        var balance = transactions.Sum(transaction => transaction.TransactionType.Equals("redeem", StringComparison.OrdinalIgnoreCase)
            ? -Math.Abs(transaction.Points)
            : transaction.Points);
        var tier = balance >= 5000 ? "Platinum" : balance >= 2500 ? "Gold" : balance >= 1000 ? "Silver" : "Bronze";
        var nextTierPoints = tier switch
        {
            "Bronze" => Math.Max(1000 - balance, 0),
            "Silver" => Math.Max(2500 - balance, 0),
            "Gold" => Math.Max(5000 - balance, 0),
            _ => 0
        };

        return new LoyaltyPointsResponse(balance, tier, nextTierPoints);
    }
}

public sealed class GetMyLoyaltyTransactionsQueryHandler : IRequestHandler<GetMyLoyaltyTransactionsQuery, IReadOnlyCollection<LoyaltyTransactionResponse>>
{
    private readonly CustomerAccountLookupService _customerAccountLookupService;
    private readonly ICustomerAppRepository _customerAppRepository;
    private readonly ICurrentUserContext _currentUserContext;

    public GetMyLoyaltyTransactionsQueryHandler(
        CustomerAccountLookupService customerAccountLookupService,
        ICustomerAppRepository customerAppRepository,
        ICurrentUserContext currentUserContext)
    {
        _customerAccountLookupService = customerAccountLookupService;
        _customerAppRepository = customerAppRepository;
        _currentUserContext = currentUserContext;
    }

    public async Task<IReadOnlyCollection<LoyaltyTransactionResponse>> Handle(GetMyLoyaltyTransactionsQuery request, CancellationToken cancellationToken)
    {
        var account = await CustomerAppAccess.ResolveCurrentCustomerAsync(_currentUserContext, _customerAccountLookupService, cancellationToken);
        var transactions = await _customerAppRepository.ListLoyaltyTransactionsAsync(account.Customer.CustomerId, cancellationToken);
        return transactions.Select(CustomerAppMapper.ToLoyaltyTransaction).ToArray();
    }
}

public sealed class GetCustomerReviewsQueryHandler : IRequestHandler<GetCustomerReviewsQuery, IReadOnlyCollection<CustomerReviewResponse>>
{
    private readonly ICustomerAppRepository _customerAppRepository;

    public GetCustomerReviewsQueryHandler(ICustomerAppRepository customerAppRepository)
    {
        _customerAppRepository = customerAppRepository;
    }

    public async Task<IReadOnlyCollection<CustomerReviewResponse>> Handle(GetCustomerReviewsQuery request, CancellationToken cancellationToken)
    {
        var reviews = await _customerAppRepository.ListReviewsAsync(request.ServiceId, cancellationToken);
        return reviews.Select(CustomerAppMapper.ToReview).ToArray();
    }
}

public sealed class CreateMyCustomerReviewCommandHandler : IRequestHandler<CreateMyCustomerReviewCommand, CustomerReviewResponse>
{
    private static readonly Domain.Enums.ServiceRequestStatus[] ReviewEligibleStatuses =
    [
        Domain.Enums.ServiceRequestStatus.WorkCompletedPendingSubmission,
        Domain.Enums.ServiceRequestStatus.SubmittedForClosure
    ];

    private readonly IBookingRepository _bookingRepository;
    private readonly CustomerAccountLookupService _customerAccountLookupService;
    private readonly ICustomerAppRepository _customerAppRepository;
    private readonly ICurrentDateTime _currentDateTime;
    private readonly ICurrentUserContext _currentUserContext;
    private readonly IUnitOfWork _unitOfWork;

    public CreateMyCustomerReviewCommandHandler(
        IBookingRepository bookingRepository,
        CustomerAccountLookupService customerAccountLookupService,
        ICustomerAppRepository customerAppRepository,
        IUnitOfWork unitOfWork,
        ICurrentDateTime currentDateTime,
        ICurrentUserContext currentUserContext)
    {
        _bookingRepository = bookingRepository;
        _customerAccountLookupService = customerAccountLookupService;
        _customerAppRepository = customerAppRepository;
        _unitOfWork = unitOfWork;
        _currentDateTime = currentDateTime;
        _currentUserContext = currentUserContext;
    }

    public async Task<CustomerReviewResponse> Handle(CreateMyCustomerReviewCommand request, CancellationToken cancellationToken)
    {
        if (request.Rating is < 1 or > 5)
        {
            throw new AppException(ErrorCodes.ValidationFailure, "Review rating must be between 1 and 5.", 400);
        }

        var account = await CustomerAppAccess.ResolveCurrentCustomerAsync(_currentUserContext, _customerAccountLookupService, cancellationToken);

        if (request.BookingId.HasValue)
        {
            var booking = await _bookingRepository.GetByIdForCustomerAsync(request.BookingId.Value, account.Customer.CustomerId, cancellationToken);

            if (booking is null)
            {
                throw new AppException(ErrorCodes.NotFound, "The requested booking could not be found for this customer.", 404);
            }

            var serviceRequestStatus = booking.ServiceRequest?.CurrentStatus;

            if (serviceRequestStatus is null || !ReviewEligibleStatuses.Contains(serviceRequestStatus.Value))
            {
                throw new AppException(ErrorCodes.ValidationFailure, "Reviews can only be submitted after the service is completed.", 400);
            }
        }

        var review = new CustomerReview
        {
            CustomerId = account.Customer.CustomerId,
            BookingId = request.BookingId,
            ServiceId = request.ServiceId,
            CustomerNameSnapshot = account.Customer.CustomerName,
            CustomerPhotoUrl = request.CustomerPhotoUrl?.Trim() ?? string.Empty,
            Rating = request.Rating,
            Comment = request.Comment.Trim(),
            IsActive = true,
            CreatedBy = CustomerAppAccess.ResolveActor(_currentUserContext),
            DateCreated = _currentDateTime.UtcNow,
            IPAddress = CustomerAppAccess.ResolveIpAddress(_currentUserContext)
        };

        await _customerAppRepository.AddReviewAsync(review, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return CustomerAppMapper.ToReview(review);
    }
}

public sealed class SubmitMyAppFeedbackCommandHandler : IRequestHandler<SubmitMyAppFeedbackCommand, CustomerAppFeedbackResponse>
{
    private readonly CustomerAccountLookupService _customerAccountLookupService;
    private readonly ICustomerAppRepository _customerAppRepository;
    private readonly ICurrentDateTime _currentDateTime;
    private readonly ICurrentUserContext _currentUserContext;
    private readonly IUnitOfWork _unitOfWork;

    public SubmitMyAppFeedbackCommandHandler(
        CustomerAccountLookupService customerAccountLookupService,
        ICustomerAppRepository customerAppRepository,
        IUnitOfWork unitOfWork,
        ICurrentDateTime currentDateTime,
        ICurrentUserContext currentUserContext)
    {
        _customerAccountLookupService = customerAccountLookupService;
        _customerAppRepository = customerAppRepository;
        _unitOfWork = unitOfWork;
        _currentDateTime = currentDateTime;
        _currentUserContext = currentUserContext;
    }

    public async Task<CustomerAppFeedbackResponse> Handle(SubmitMyAppFeedbackCommand request, CancellationToken cancellationToken)
    {
        var account = await CustomerAppAccess.ResolveCurrentCustomerAsync(_currentUserContext, _customerAccountLookupService, cancellationToken);
        var feedback = new CustomerAppFeedback
        {
            CustomerId = account.Customer.CustomerId,
            FeedbackType = request.FeedbackType?.Trim() ?? "general",
            Message = request.Message.Trim(),
            Rating = request.Rating,
            AppVersion = request.AppVersion?.Trim() ?? string.Empty,
            DeviceInfo = request.DeviceInfo?.Trim() ?? string.Empty,
            FeedbackStatus = "Submitted",
            CreatedBy = CustomerAppAccess.ResolveActor(_currentUserContext),
            DateCreated = _currentDateTime.UtcNow,
            IPAddress = CustomerAppAccess.ResolveIpAddress(_currentUserContext)
        };

        await _customerAppRepository.AddAppFeedbackAsync(feedback, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return new CustomerAppFeedbackResponse(
            feedback.CustomerAppFeedbackId,
            feedback.CustomerId,
            feedback.FeedbackType,
            feedback.Message,
            feedback.Rating,
            feedback.AppVersion,
            feedback.DeviceInfo,
            feedback.FeedbackStatus,
            feedback.DateCreated);
    }
}

public sealed class RescheduleMyCustomerBookingCommandHandler : IRequestHandler<RescheduleMyCustomerBookingCommand, BookingDetailResponse>
{
    private readonly IBookingLookupRepository _bookingLookupRepository;
    private readonly IBookingRepository _bookingRepository;
    private readonly CustomerAccountLookupService _customerAccountLookupService;
    private readonly ICurrentDateTime _currentDateTime;
    private readonly ICurrentUserContext _currentUserContext;
    private readonly IFieldLookupRepository _fieldLookupRepository;
    private readonly IUnitOfWork _unitOfWork;

    public RescheduleMyCustomerBookingCommandHandler(
        CustomerAccountLookupService customerAccountLookupService,
        IBookingRepository bookingRepository,
        IBookingLookupRepository bookingLookupRepository,
        IFieldLookupRepository fieldLookupRepository,
        IUnitOfWork unitOfWork,
        ICurrentDateTime currentDateTime,
        ICurrentUserContext currentUserContext)
    {
        _customerAccountLookupService = customerAccountLookupService;
        _bookingRepository = bookingRepository;
        _bookingLookupRepository = bookingLookupRepository;
        _fieldLookupRepository = fieldLookupRepository;
        _unitOfWork = unitOfWork;
        _currentDateTime = currentDateTime;
        _currentUserContext = currentUserContext;
    }

    public async Task<BookingDetailResponse> Handle(RescheduleMyCustomerBookingCommand request, CancellationToken cancellationToken)
    {
        var account = await CustomerAppAccess.ResolveCurrentCustomerAsync(_currentUserContext, _customerAccountLookupService, cancellationToken);
        var booking = await _bookingRepository.GetByIdForCustomerForUpdateAsync(request.BookingId, account.Customer.CustomerId, cancellationToken)
            ?? throw new AppException(ErrorCodes.BookingAccessDenied, "This booking does not belong to the current customer.", 403);
        var newSlot = await _bookingLookupRepository.GetSlotAvailabilityByIdAsync(request.SlotAvailabilityId, cancellationToken)
            ?? throw new AppException(ErrorCodes.SlotUnavailable, "The selected slot is unavailable.", 409);

        if (newSlot.IsBlocked || newSlot.ReservedCapacity >= newSlot.AvailableCapacity)
        {
            throw new AppException(ErrorCodes.SlotUnavailable, "The selected slot is unavailable.", 409);
        }

        if (booking.SlotAvailability is not null && booking.SlotAvailability.ReservedCapacity > 0)
        {
            booking.SlotAvailability.ReservedCapacity -= 1;
        }

        newSlot.ReservedCapacity += 1;
        booking.SlotAvailability = newSlot;
        booking.SlotAvailabilityId = newSlot.SlotAvailabilityId;
        booking.ZoneId = newSlot.ZoneId;
        booking.ZoneNameSnapshot = newSlot.Zone?.ZoneName ?? booking.ZoneNameSnapshot;
        booking.LastUpdated = _currentDateTime.UtcNow;
        booking.UpdatedBy = CustomerAppAccess.ResolveActor(_currentUserContext);
        booking.BookingStatusHistories.Add(new BookingStatusHistory
        {
            BookingStatus = BookingStatus.Confirmed,
            Remarks = string.IsNullOrWhiteSpace(request.Remarks) ? "Customer booking rescheduled." : request.Remarks.Trim(),
            StatusDateUtc = _currentDateTime.UtcNow,
            CreatedBy = CustomerAppAccess.ResolveActor(_currentUserContext),
            DateCreated = _currentDateTime.UtcNow,
            IPAddress = CustomerAppAccess.ResolveIpAddress(_currentUserContext)
        });

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var serviceId = booking.BookingLines
            .OrderBy(line => line.BookingLineId)
            .Select(line => line.ServiceId)
            .FirstOrDefault();
        var checklistMasters = serviceId > 0
            ? await _fieldLookupRepository.GetChecklistByServiceIdAsync(serviceId, cancellationToken)
            : Array.Empty<ServiceChecklistMaster>();

        return BookingResponseMapper.ToDetail(booking, checklistMasters);
    }
}

public sealed class GetCustomerVisibleTechnicianQueryHandler : IRequestHandler<GetCustomerVisibleTechnicianQuery, CustomerVisibleTechnicianResponse>
{
    private readonly ITechnicianRepository _technicianRepository;

    public GetCustomerVisibleTechnicianQueryHandler(ITechnicianRepository technicianRepository)
    {
        _technicianRepository = technicianRepository;
    }

    public async Task<CustomerVisibleTechnicianResponse> Handle(GetCustomerVisibleTechnicianQuery request, CancellationToken cancellationToken)
    {
        var technician = await _technicianRepository.GetByIdAsync(request.TechnicianId, cancellationToken)
            ?? throw new AppException(ErrorCodes.NotFound, "The technician could not be found.", 404);
        var totalJobs = technician.ServiceRequestAssignments.Count(assignment => !assignment.IsDeleted);

        return new CustomerVisibleTechnicianResponse(
            technician.TechnicianId,
            technician.TechnicianName,
            string.Empty,
            0,
            totalJobs,
            "Verified Coolzo technician",
            technician.BaseZone is null ? Array.Empty<string>() : new[] { technician.BaseZone.ZoneName },
            new[] { "English", "Hindi" },
            technician.IsActive);
    }
}

public sealed class GetPublicBlogsQueryHandler : IRequestHandler<GetPublicBlogsQuery, IReadOnlyCollection<BlogContentResponse>>
{
    private readonly IAdminConfigurationRepository _adminConfigurationRepository;

    public GetPublicBlogsQueryHandler(IAdminConfigurationRepository adminConfigurationRepository)
    {
        _adminConfigurationRepository = adminConfigurationRepository;
    }

    public async Task<IReadOnlyCollection<BlogContentResponse>> Handle(GetPublicBlogsQuery request, CancellationToken cancellationToken)
    {
        var blocks = await _adminConfigurationRepository.SearchCmsBlocksAsync("blog:", true, true, cancellationToken);
        return blocks
            .Where(block => block.BlockKey.StartsWith("blog:", StringComparison.OrdinalIgnoreCase))
            .Select(CustomerAppMapper.ToBlog)
            .ToArray();
    }
}

public sealed class GetPublicBlogByIdQueryHandler : IRequestHandler<GetPublicBlogByIdQuery, BlogContentResponse?>
{
    private readonly IAdminConfigurationRepository _adminConfigurationRepository;

    public GetPublicBlogByIdQueryHandler(IAdminConfigurationRepository adminConfigurationRepository)
    {
        _adminConfigurationRepository = adminConfigurationRepository;
    }

    public async Task<BlogContentResponse?> Handle(GetPublicBlogByIdQuery request, CancellationToken cancellationToken)
    {
        var key = request.Id.Trim();
        var block = await _adminConfigurationRepository.GetCmsBlockByKeyAsync(key, true, cancellationToken)
            ?? await _adminConfigurationRepository.GetCmsBlockByKeyAsync($"blog:{key}", true, cancellationToken);

        return block is null ? null : CustomerAppMapper.ToBlog(block);
    }
}

public sealed class GetPublicChangelogQueryHandler : IRequestHandler<GetPublicChangelogQuery, IReadOnlyCollection<ChangelogItemResponse>>
{
    private readonly IAdminConfigurationRepository _adminConfigurationRepository;

    public GetPublicChangelogQueryHandler(IAdminConfigurationRepository adminConfigurationRepository)
    {
        _adminConfigurationRepository = adminConfigurationRepository;
    }

    public async Task<IReadOnlyCollection<ChangelogItemResponse>> Handle(GetPublicChangelogQuery request, CancellationToken cancellationToken)
    {
        var blocks = await _adminConfigurationRepository.SearchCmsBlocksAsync("changelog:", true, true, cancellationToken);
        return blocks
            .Where(block => block.BlockKey.StartsWith("changelog:", StringComparison.OrdinalIgnoreCase))
            .Select(CustomerAppMapper.ToChangelog)
            .ToArray();
    }
}
