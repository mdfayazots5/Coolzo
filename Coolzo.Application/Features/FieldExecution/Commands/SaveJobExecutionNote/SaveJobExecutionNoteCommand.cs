using Coolzo.Contracts.Responses.FieldExecution;
using MediatR;

namespace Coolzo.Application.Features.FieldExecution.Commands.SaveJobExecutionNote;

public sealed record SaveJobExecutionNoteCommand(
    long ServiceRequestId,
    string NoteText,
    bool IsCustomerVisible) : IRequest<JobExecutionNoteResponse>;
