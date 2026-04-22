using Coolzo.Contracts.Responses.Booking;
using MediatR;

namespace Coolzo.Application.Features.BookingLookup.Queries.GetZones;

public sealed record GetZonesQuery(string? Search) : IRequest<IReadOnlyCollection<ZoneListItemResponse>>;
