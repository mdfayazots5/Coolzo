using Coolzo.Contracts.Responses.Booking;
using MediatR;

namespace Coolzo.Application.Features.BookingLookup.Queries.GetServices;

public sealed record GetServicesQuery(long? ServiceCategoryId, string? Search) : IRequest<IReadOnlyCollection<ServiceLookupResponse>>;
