namespace Coolzo.Domain.Entities;

public sealed class CustomerEquipment : AuditableEntity
{
    public long CustomerEquipmentId { get; set; }

    public long CustomerId { get; set; }

    public string EquipmentName { get; set; } = string.Empty;

    public string EquipmentType { get; set; } = string.Empty;

    public string BrandName { get; set; } = string.Empty;

    public string Capacity { get; set; } = string.Empty;

    public string LocationLabel { get; set; } = string.Empty;

    public DateOnly? PurchaseDate { get; set; }

    public DateOnly? LastServiceDate { get; set; }

    public string SerialNumber { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;

    public Customer? Customer { get; set; }
}

public sealed class CustomerNotification : AuditableEntity
{
    public long CustomerNotificationId { get; set; }

    public long CustomerId { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Message { get; set; } = string.Empty;

    public string NotificationType { get; set; } = "info";

    public bool IsRead { get; set; }

    public string LinkUrl { get; set; } = string.Empty;

    public Customer? Customer { get; set; }
}

public sealed class PromotionalOffer : AuditableEntity
{
    public long PromotionalOfferId { get; set; }

    public string OfferCode { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string DiscountType { get; set; } = "fixed";

    public decimal DiscountValue { get; set; }

    public decimal MinimumOrderValue { get; set; }

    public DateOnly? ExpiryDate { get; set; }

    public string Category { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;
}

public sealed class CustomerReferral : AuditableEntity
{
    public long CustomerReferralId { get; set; }

    public long CustomerId { get; set; }

    public string ReferralName { get; set; } = string.Empty;

    public string ReferralStatus { get; set; } = "Pending";

    public decimal RewardAmount { get; set; }

    public DateOnly ReferralDate { get; set; }

    public Customer? Customer { get; set; }
}

public sealed class CustomerLoyaltyTransaction : AuditableEntity
{
    public long CustomerLoyaltyTransactionId { get; set; }

    public long CustomerId { get; set; }

    public string TransactionType { get; set; } = "earn";

    public int Points { get; set; }

    public string Description { get; set; } = string.Empty;

    public Customer? Customer { get; set; }
}

public sealed class CustomerReview : AuditableEntity
{
    public long CustomerReviewId { get; set; }

    public long CustomerId { get; set; }

    public long? BookingId { get; set; }

    public long? ServiceId { get; set; }

    public string CustomerNameSnapshot { get; set; } = string.Empty;

    public string CustomerPhotoUrl { get; set; } = string.Empty;

    public int Rating { get; set; }

    public string Comment { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;

    public Customer? Customer { get; set; }
}

public sealed class CustomerAppFeedback : AuditableEntity
{
    public long CustomerAppFeedbackId { get; set; }

    public long CustomerId { get; set; }

    public string FeedbackType { get; set; } = "general";

    public string Message { get; set; } = string.Empty;

    public int? Rating { get; set; }

    public string AppVersion { get; set; } = string.Empty;

    public string DeviceInfo { get; set; } = string.Empty;

    public string FeedbackStatus { get; set; } = "Submitted";

    public Customer? Customer { get; set; }
}
