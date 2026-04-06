using Coolzo.Contracts.Responses.TechnicianJobs;
using MediatR;

namespace Coolzo.Application.Features.FieldExecution.Commands.StartTechnicianWork;

public sealed record StartTechnicianWorkCommand(
    long ServiceRequestId,
    string? Remarks,
    string? WorkSummary) : IRequest<TechnicianJobDetailResponse>;
