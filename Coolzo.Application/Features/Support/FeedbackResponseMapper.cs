using Coolzo.Contracts.Responses.Support;
using Coolzo.Domain.Entities;

namespace Coolzo.Application.Features.Support;

public static class FeedbackResponseMapper
{
    private const string FlaggedPrefix = "flagged|";

    public static SupportFeedbackResponse ToResponse(CustomerReview review)
    {
        return new SupportFeedbackResponse(
            review.CustomerReviewId,
            review.CustomerId,
            review.CustomerNameSnapshot,
            review.CustomerPhotoUrl,
            review.Rating,
            review.Comment,
            review.BookingId,
            review.ServiceId,
            review.DateCreated,
            ResolveStatus(review),
            string.IsNullOrWhiteSpace(review.Comments) ? null : review.Comments,
            ExtractFlagReason(review.Tag),
            review.LastUpdated ?? review.DatePublished,
            review.UpdatedBy ?? review.PublishedBy);
    }

    public static string ResolveStatus(CustomerReview review)
    {
        if (IsFlagged(review.Tag))
        {
            return "flagged";
        }

        return review.IsPublished && review.DisplayOnWeb ? "published" : "unpublished";
    }

    public static string BuildFlagTag(string reason)
    {
        var sanitizedReason = string.IsNullOrWhiteSpace(reason)
            ? "Flagged by support"
            : reason.Trim();

        var maxReasonLength = 64 - FlaggedPrefix.Length;
        if (sanitizedReason.Length > maxReasonLength)
        {
            sanitizedReason = sanitizedReason[..maxReasonLength];
        }

        return $"{FlaggedPrefix}{sanitizedReason}";
    }

    private static string? ExtractFlagReason(string? tag)
    {
        if (!IsFlagged(tag))
        {
            return null;
        }

        var reason = tag![FlaggedPrefix.Length..].Trim();
        return reason.Length == 0 ? null : reason;
    }

    private static bool IsFlagged(string? tag)
    {
        return !string.IsNullOrWhiteSpace(tag) &&
            tag.StartsWith(FlaggedPrefix, StringComparison.OrdinalIgnoreCase);
    }
}
