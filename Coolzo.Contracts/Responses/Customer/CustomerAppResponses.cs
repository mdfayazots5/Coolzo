namespace Coolzo.Contracts.Responses.Customer;

public sealed record CustomerProfileResponse(
    long CustomerId,
    long? UserId,
    string CustomerName,
    string MobileNumber,
    string EmailAddress,
    string PhotoUrl,
    string MembershipStatus,
    bool IsActive,
    DateTime DateCreated,
    DateTime? LastUpdated);

public sealed record CustomerAccountDeletionResponse(
    long CustomerId,
    bool IsActive,
    DateTime DeactivatedAtUtc,
    string Reason);

public sealed record CustomerEquipmentResponse(
    long CustomerEquipmentId,
    long CustomerId,
    string Name,
    string Type,
    string Brand,
    string Capacity,
    string Location,
    DateOnly? PurchaseDate,
    DateOnly? LastServiceDate,
    string SerialNumber,
    bool IsActive,
    DateTime DateCreated,
    DateTime? LastUpdated);

public sealed record CustomerNotificationResponse(
    long CustomerNotificationId,
    long CustomerId,
    string Title,
    string Message,
    string Type,
    bool IsRead,
    DateTime CreatedAt,
    string Link);

public sealed record PromotionalOfferResponse(
    long PromotionalOfferId,
    string Code,
    string Title,
    string Description,
    string DiscountType,
    decimal DiscountValue,
    decimal MinOrderValue,
    DateOnly? ExpiryDate,
    string Category);

public sealed record ReferralResponse(
    long CustomerReferralId,
    string Name,
    string Status,
    decimal Reward,
    DateOnly Date);

public sealed record ReferralStatsResponse(
    string ReferralCode,
    int TotalReferrals,
    decimal TotalEarnings,
    int PendingReferrals,
    IReadOnlyCollection<ReferralResponse> Referrals);

public sealed record LoyaltyPointsResponse(
    int Balance,
    string Tier,
    int NextTierPoints);

public sealed record LoyaltyTransactionResponse(
    long CustomerLoyaltyTransactionId,
    string Type,
    int Points,
    string Description,
    DateTime CreatedAt);

public sealed record CustomerReviewResponse(
    long CustomerReviewId,
    long CustomerId,
    string UserName,
    string UserPhoto,
    int Rating,
    string Comment,
    long? BookingId,
    long? ServiceId,
    DateTime CreatedAt);

public sealed record CustomerAppFeedbackResponse(
    long CustomerAppFeedbackId,
    long CustomerId,
    string FeedbackType,
    string Message,
    int? Rating,
    string AppVersion,
    string DeviceInfo,
    string FeedbackStatus,
    DateTime CreatedAt);

public sealed record CustomerVisibleTechnicianResponse(
    long TechnicianId,
    string Name,
    string PhotoUrl,
    decimal Rating,
    int TotalJobs,
    string Experience,
    IReadOnlyCollection<string> Specialization,
    IReadOnlyCollection<string> Languages,
    bool Verified);
