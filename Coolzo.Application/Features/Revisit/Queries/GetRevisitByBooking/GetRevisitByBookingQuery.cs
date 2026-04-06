using Coolzo.Contracts.Responses.Revisit;
using MediatR;

namespace Coolzo.Application.Features.Revisit.Queries.GetRevisitByBooking;

public sealed record GetRevisitByBookingQuery(long BookingId) : IRequest<IReadOnlyCollection<RevisitRequestResponse>>;
