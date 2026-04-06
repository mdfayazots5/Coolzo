using Asp.Versioning;
using Coolzo.Application.Features.FieldExecution.Commands.MarkTechnicianJobEnRoute;
using Coolzo.Application.Features.FieldExecution.Commands.MarkTechnicianJobReached;
using Coolzo.Application.Features.FieldExecution.Commands.MarkTechnicianWorkCompleted;
using Coolzo.Application.Features.FieldExecution.Commands.MarkTechnicianWorkInProgress;
using Coolzo.Application.Features.FieldExecution.Commands.SaveJobExecutionNote;
using Coolzo.Application.Features.FieldExecution.Commands.StartTechnicianWork;
using Coolzo.Application.Features.FieldExecution.Commands.SubmitTechnicianJobForClosure;
using Coolzo.Application.Features.FieldExecution.Queries.GetJobExecutionTimeline;
using Coolzo.Contracts.Common;
using Coolzo.Contracts.Requests.FieldExecution;
using Coolzo.Contracts.Responses.FieldExecution;
using Coolzo.Contracts.Responses.TechnicianJobs;
using Coolzo.Shared.Constants;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Coolzo.Api.Controllers;

[ApiVersion("1.0")]
[Authorize(Roles = RoleNames.Technician)]
[Route("api/v{version:apiVersion}/technician-jobs")]
public sealed class FieldExecutionController : ApiControllerBase
{
    private readonly ISender _sender;

    public FieldExecutionController(ISender sender)
    {
        _sender = sender;
    }

    [HttpPost("{id:long}/mark-enroute")]
    [ProducesResponseType(typeof(ApiResponse<TechnicianJobDetailResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<TechnicianJobDetailResponse>>> MarkEnRouteAsync(
        [FromRoute] long id,
        [FromBody] UpdateTechnicianJobStatusRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _sender.Send(
            new MarkTechnicianJobEnRouteCommand(id, request.Remarks, request.WorkSummary),
            cancellationToken);

        return Success(response, "Technician job marked en route successfully.");
    }

    [HttpPost("{id:long}/mark-reached")]
    [ProducesResponseType(typeof(ApiResponse<TechnicianJobDetailResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<TechnicianJobDetailResponse>>> MarkReachedAsync(
        [FromRoute] long id,
        [FromBody] UpdateTechnicianJobStatusRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _sender.Send(
            new MarkTechnicianJobReachedCommand(id, request.Remarks, request.WorkSummary),
            cancellationToken);

        return Success(response, "Technician job marked reached successfully.");
    }

    [HttpPost("{id:long}/start-work")]
    [ProducesResponseType(typeof(ApiResponse<TechnicianJobDetailResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<TechnicianJobDetailResponse>>> StartWorkAsync(
        [FromRoute] long id,
        [FromBody] UpdateTechnicianJobStatusRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _sender.Send(
            new StartTechnicianWorkCommand(id, request.Remarks, request.WorkSummary),
            cancellationToken);

        return Success(response, "Technician work started successfully.");
    }

    [HttpPost("{id:long}/mark-in-progress")]
    [ProducesResponseType(typeof(ApiResponse<TechnicianJobDetailResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<TechnicianJobDetailResponse>>> MarkInProgressAsync(
        [FromRoute] long id,
        [FromBody] UpdateTechnicianJobStatusRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _sender.Send(
            new MarkTechnicianWorkInProgressCommand(id, request.Remarks, request.WorkSummary),
            cancellationToken);

        return Success(response, "Technician job marked in progress successfully.");
    }

    [HttpPost("{id:long}/mark-work-completed")]
    [ProducesResponseType(typeof(ApiResponse<TechnicianJobDetailResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<TechnicianJobDetailResponse>>> MarkWorkCompletedAsync(
        [FromRoute] long id,
        [FromBody] UpdateTechnicianJobStatusRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _sender.Send(
            new MarkTechnicianWorkCompletedCommand(id, request.Remarks, request.WorkSummary),
            cancellationToken);

        return Success(response, "Technician job marked work completed successfully.");
    }

    [HttpPost("{id:long}/submit-for-closure")]
    [ProducesResponseType(typeof(ApiResponse<TechnicianJobDetailResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<TechnicianJobDetailResponse>>> SubmitForClosureAsync(
        [FromRoute] long id,
        [FromBody] UpdateTechnicianJobStatusRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _sender.Send(
            new SubmitTechnicianJobForClosureCommand(id, request.Remarks, request.WorkSummary),
            cancellationToken);

        return Success(response, "Technician job submitted for closure successfully.");
    }

    [HttpPost("{id:long}/notes")]
    [ProducesResponseType(typeof(ApiResponse<JobExecutionNoteResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<JobExecutionNoteResponse>>> SaveNoteAsync(
        [FromRoute] long id,
        [FromBody] SaveJobExecutionNoteRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _sender.Send(
            new SaveJobExecutionNoteCommand(id, request.NoteText, request.IsCustomerVisible),
            cancellationToken);

        return Success(response, "Technician note saved successfully.");
    }

    [HttpGet("{id:long}/timeline")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyCollection<JobExecutionTimelineItemResponse>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IReadOnlyCollection<JobExecutionTimelineItemResponse>>>> GetTimelineAsync(
        [FromRoute] long id,
        CancellationToken cancellationToken)
    {
        var response = await _sender.Send(new GetJobExecutionTimelineQuery(id), cancellationToken);

        return Success(response);
    }
}
