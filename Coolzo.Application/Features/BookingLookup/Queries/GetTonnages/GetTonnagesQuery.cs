using Coolzo.Contracts.Responses.Booking;
using MediatR;

namespace Coolzo.Application.Features.BookingLookup.Queries.GetTonnages;

public sealed record GetTonnagesQuery(string? Search) : IRequest<IReadOnlyCollection<TonnageLookupResponse>>;
