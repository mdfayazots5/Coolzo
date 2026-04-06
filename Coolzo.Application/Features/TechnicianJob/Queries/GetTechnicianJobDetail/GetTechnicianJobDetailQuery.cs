using Coolzo.Contracts.Responses.TechnicianJobs;
using MediatR;

namespace Coolzo.Application.Features.TechnicianJob.Queries.GetTechnicianJobDetail;

public sealed record GetTechnicianJobDetailQuery(
    long ServiceRequestId) : IRequest<TechnicianJobDetailResponse>;
