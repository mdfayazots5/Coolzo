using Coolzo.Contracts.Common;
using Coolzo.Contracts.Responses.TechnicianJobs;
using MediatR;

namespace Coolzo.Application.Features.TechnicianJob.Queries.GetTechnicianOwnJobList;

public sealed record GetTechnicianOwnJobListQuery(
    string? Status,
    DateOnly? SlotDate,
    int PageNumber,
    int PageSize) : IRequest<PagedResult<TechnicianJobListItemResponse>>;
