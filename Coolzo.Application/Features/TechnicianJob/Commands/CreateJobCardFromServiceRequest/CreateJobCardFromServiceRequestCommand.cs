using Coolzo.Contracts.Responses.FieldExecution;
using MediatR;

namespace Coolzo.Application.Features.TechnicianJob.Commands.CreateJobCardFromServiceRequest;

public sealed record CreateJobCardFromServiceRequestCommand(
    long ServiceRequestId) : IRequest<JobCardSummaryResponse>;
