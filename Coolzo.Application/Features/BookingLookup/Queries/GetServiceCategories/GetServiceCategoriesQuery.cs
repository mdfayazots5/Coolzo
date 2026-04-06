using Coolzo.Contracts.Responses.Booking;
using MediatR;

namespace Coolzo.Application.Features.BookingLookup.Queries.GetServiceCategories;

public sealed record GetServiceCategoriesQuery(string? Search) : IRequest<IReadOnlyCollection<ServiceCategoryLookupResponse>>;
