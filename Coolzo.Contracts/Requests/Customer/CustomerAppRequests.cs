namespace Coolzo.Contracts.Requests.Customer;

public sealed record UpdateCustomerProfileRequest(
    string CustomerName,
    string MobileNumber,
    string EmailAddress,
    string? PhotoUrl,
    string? MembershipStatus);

public sealed record DeleteCustomerAccountRequest(string? Reason);

public sealed record CreateCustomerEquipmentRequest(
    string Name,
    string Type,
    string Brand,
    string Capacity,
    string Location,
    DateOnly? PurchaseDate,
    DateOnly? LastServiceDate,
    string? SerialNumber);

public sealed record UpdateCustomerEquipmentRequest(
    long CustomerEquipmentId,
    string Name,
    string Type,
    string Brand,
    string Capacity,
    string Location,
    DateOnly? PurchaseDate,
    DateOnly? LastServiceDate,
    string? SerialNumber);

public sealed record ValidateCouponRequest(string Code);

public sealed record CreateCustomerReviewRequest(
    int Rating,
    string Comment,
    long? BookingId,
    long? ServiceId,
    string? CustomerPhotoUrl);

public sealed record SubmitAppFeedbackRequest(
    string? FeedbackType,
    string Message,
    int? Rating,
    string? AppVersion,
    string? DeviceInfo);

public sealed record RescheduleCustomerBookingRequest(
    long SlotAvailabilityId,
    string? Remarks);
