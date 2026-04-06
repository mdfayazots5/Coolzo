using Coolzo.Contracts.Responses.Operations;
using MediatR;

namespace Coolzo.Application.Features.Technician.Queries.GetTechnicianList;

public sealed record GetTechnicianListQuery(
    string? SearchTerm,
    bool ActiveOnly) : IRequest<IReadOnlyCollection<TechnicianListItemResponse>>;
