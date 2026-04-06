using Coolzo.Contracts.Responses.Booking;
using MediatR;

namespace Coolzo.Application.Features.BookingLookup.Queries.GetBrands;

public sealed record GetBrandsQuery(string? Search) : IRequest<IReadOnlyCollection<BrandLookupResponse>>;
