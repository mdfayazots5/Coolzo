using Coolzo.Contracts.Responses.Amc;
using MediatR;

namespace Coolzo.Application.Features.Amc.Queries.GetCustomerAmc;

public sealed record GetCustomerAmcQuery(long CustomerId) : IRequest<IReadOnlyCollection<CustomerAmcResponse>>;
