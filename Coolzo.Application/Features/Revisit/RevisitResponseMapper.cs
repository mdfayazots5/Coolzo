using Coolzo.Contracts.Responses.Revisit;
using Coolzo.Domain.Entities;

namespace Coolzo.Application.Features.Revisit;

internal static class RevisitResponseMapper
{
    public static RevisitRequestResponse ToResponse(RevisitRequest revisitRequest)
    {
        return new RevisitRequestResponse(
            revisitRequest.RevisitRequestId,
            revisitRequest.BookingId,
            revisitRequest.Booking?.BookingReference ??
                revisitRequest.OriginalServiceRequest?.Booking?.BookingReference ??
                string.Empty,
            revisitRequest.CustomerId,
            revisitRequest.Customer?.CustomerName ??
                revisitRequest.Booking?.CustomerNameSnapshot ??
                revisitRequest.OriginalServiceRequest?.Booking?.CustomerNameSnapshot ??
                string.Empty,
            revisitRequest.OriginalJobCardId,
            revisitRequest.OriginalJobCard?.JobCardNumber ?? string.Empty,
            revisitRequest.RevisitType.ToString(),
            revisitRequest.CurrentStatus.ToString(),
            revisitRequest.RequestedDateUtc,
            revisitRequest.PreferredVisitDateUtc,
            revisitRequest.IssueSummary,
            revisitRequest.RequestRemarks,
            revisitRequest.ChargeAmount,
            revisitRequest.CustomerAmcId,
            revisitRequest.WarrantyClaimId);
    }
}
