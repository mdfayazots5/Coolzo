namespace Coolzo.Contracts.Responses.Support;

public sealed record SupportFeedbackResponse(
    long CustomerReviewId,
    long CustomerId,
    string CustomerName,
    string CustomerPhotoUrl,
    int Rating,
    string Comment,
    long? BookingId,
    long? ServiceId,
    DateTime CreatedAt,
    string FeedbackStatus,
    string? AdminResponse,
    string? FlagReason,
    DateTime? ModeratedAt,
    string? ModeratedBy);
