using Coolzo.Contracts.Responses.FieldExecution;
using MediatR;

namespace Coolzo.Application.Features.ServiceRequest.Commands.SaveServiceRequestNote;

public sealed record SaveServiceRequestNoteCommand(
    long ServiceRequestId,
    string NoteText,
    bool IsCustomerVisible) : IRequest<JobExecutionNoteResponse>;
