using Coolzo.Contracts.Responses.Booking;
using MediatR;

namespace Coolzo.Application.Features.BookingLookup.Queries.GetAcTypes;

public sealed record GetAcTypesQuery(string? Search) : IRequest<IReadOnlyCollection<AcTypeLookupResponse>>;
