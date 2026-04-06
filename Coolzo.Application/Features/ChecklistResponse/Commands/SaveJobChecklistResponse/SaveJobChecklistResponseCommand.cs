using Coolzo.Contracts.Requests.FieldExecution;
using Coolzo.Contracts.Responses.FieldExecution;
using MediatR;

namespace Coolzo.Application.Features.ChecklistResponse.Commands.SaveJobChecklistResponse;

public sealed record SaveJobChecklistResponseCommand(
    long ServiceRequestId,
    IReadOnlyCollection<SaveJobChecklistResponseItemRequest> Items) : IRequest<IReadOnlyCollection<JobChecklistItemResponse>>;
