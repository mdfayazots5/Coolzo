using Coolzo.Contracts.Responses.TechnicianJobs;
using MediatR;

namespace Coolzo.Application.Features.FieldExecution.Commands.MarkTechnicianWorkCompleted;

public sealed record MarkTechnicianWorkCompletedCommand(
    long ServiceRequestId,
    string? Remarks,
    string? WorkSummary) : IRequest<TechnicianJobDetailResponse>;
