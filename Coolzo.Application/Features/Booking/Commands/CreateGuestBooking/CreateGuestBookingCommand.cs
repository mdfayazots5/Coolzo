using Coolzo.Contracts.Responses.Booking;
using MediatR;

namespace Coolzo.Application.Features.Booking.Commands.CreateGuestBooking;

public sealed record CreateGuestBookingCommand(
    long ServiceId,
    long AcTypeId,
    long TonnageId,
    long BrandId,
    long SlotAvailabilityId,
    string CustomerName,
    string MobileNumber,
    string? EmailAddress,
    string AddressLine1,
    string? AddressLine2,
    string? Landmark,
    string CityName,
    string Pincode,
    string? AddressLabel,
    string? ModelName,
    string? IssueNotes,
    string SourceChannel,
    string? IdempotencyKey) : IRequest<BookingSummaryResponse>;
