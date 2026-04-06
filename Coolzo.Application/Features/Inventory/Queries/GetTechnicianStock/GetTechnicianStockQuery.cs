using Coolzo.Contracts.Responses.Inventory;
using MediatR;

namespace Coolzo.Application.Features.Inventory.Queries.GetTechnicianStock;

public sealed record GetTechnicianStockQuery(long TechnicianId) : IRequest<TechnicianStockResponse>;
