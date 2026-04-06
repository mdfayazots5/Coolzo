using Coolzo.Contracts.Responses.TechnicianJobs;
using MediatR;

namespace Coolzo.Application.Features.FieldExecution.Commands.MarkTechnicianWorkInProgress;

public sealed record MarkTechnicianWorkInProgressCommand(
    long ServiceRequestId,
    string? Remarks,
    string? WorkSummary) : IRequest<TechnicianJobDetailResponse>;
