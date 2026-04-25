using Coolzo.Api.Extensions;
using Coolzo.Application.Features.Support.Commands.AssignSupportTicket;
using Coolzo.Application.Features.Support.Commands.ChangeSupportTicketPriority;
using Coolzo.Application.Features.Support.Commands.ChangeSupportTicketStatus;
using Coolzo.Application.Features.Support.Commands.CloseSupportTicket;
using Coolzo.Application.Features.Support.Commands.CreateSupportTicket;
using Coolzo.Application.Features.Support.Commands.ReopenSupportTicket;
using Coolzo.Application.Features.Support.Queries.GetCustomerSupportTicketList;
using Coolzo.Application.Features.Support.Queries.GetSupportTicketDetail;
using Coolzo.Application.Features.Support.Queries.GetSupportTicketList;
using Coolzo.Contracts.Common;
using Coolzo.Contracts.Requests.Support;
using Coolzo.Contracts.Responses.Support;
using Coolzo.Shared.Constants;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Coolzo.Api.Controllers;

[Authorize]
[Route("api/support-tickets")]
public sealed class SupportTicketController : ApiControllerBase
{
    private readonly ISender _sender;

    public SupportTicketController(ISender sender)
    {
        _sender = sender;
    }

    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<SupportTicketDetailResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<SupportTicketDetailResponse>>> CreateAsync(
        [FromBody] CreateSupportTicketRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _sender.Send(
            new CreateSupportTicketCommand(
                request.CustomerId,
                request.Subject,
                request.CategoryId,
                request.PriorityId,
                request.Description,
                request.Links ?? Array.Empty<CreateSupportTicketLinkRequest>()),
            cancellationToken);

        return Success(response, "Support ticket created successfully.");
    }

    [HttpGet]
    [Authorize(Policy = PermissionNames.SupportRead)]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<SupportTicketListItemResponse>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<PagedResult<SupportTicketListItemResponse>>>> SearchAsync(
        [FromQuery] string? ticketNumber,
        [FromQuery] string? customerMobile,
        [FromQuery] long? categoryId,
        [FromQuery] long? priorityId,
        [FromQuery] string? status,
        [FromQuery] DateOnly? dateFrom,
        [FromQuery] DateOnly? dateTo,
        [FromQuery] string? linkedEntityType,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var response = await _sender.Send(
            new GetSupportTicketListQuery(
                ticketNumber,
                customerMobile,
                categoryId,
                priorityId,
                status,
                dateFrom,
                dateTo,
                linkedEntityType,
                pageNumber,
                pageSize),
            cancellationToken);

        return Success(response);
    }

    [HttpGet("{supportTicketId:long}")]
    [ProducesResponseType(typeof(ApiResponse<SupportTicketDetailResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<SupportTicketDetailResponse>>> GetByIdAsync(
        [FromRoute] long supportTicketId,
        CancellationToken cancellationToken)
    {
        var response = await _sender.Send(new GetSupportTicketDetailQuery(supportTicketId), cancellationToken);

        return Success(response);
    }

    [HttpGet("my-tickets")]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<SupportTicketListItemResponse>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<SupportTicketCountResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMyTicketsAsync(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] bool countOnly = false,
        [FromQuery] bool unread = false,
        CancellationToken cancellationToken = default)
    {
        var response = await _sender.Send(new GetCustomerSupportTicketListQuery(pageNumber, pageSize, countOnly, unread), cancellationToken);

        if (countOnly)
        {
            return Ok(ApiResponseFactory.Success(new SupportTicketCountResponse(response.TotalCount), HttpContext.TraceIdentifier));
        }

        return Ok(ApiResponseFactory.Success(response, HttpContext.TraceIdentifier));
    }

    [HttpPost("{supportTicketId:long}/assign")]
    [Authorize(Policy = PermissionNames.SupportManage)]
    [ProducesResponseType(typeof(ApiResponse<SupportTicketDetailResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<SupportTicketDetailResponse>>> AssignAsync(
        [FromRoute] long supportTicketId,
        [FromBody] AssignSupportTicketRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _sender.Send(
            new AssignSupportTicketCommand(supportTicketId, request.AssignedUserId, request.Remarks),
            cancellationToken);

        return Success(response, "Support ticket assigned successfully.");
    }

    [HttpPost("{supportTicketId:long}/change-status")]
    [Authorize(Policy = PermissionNames.SupportManage)]
    [ProducesResponseType(typeof(ApiResponse<SupportTicketDetailResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<SupportTicketDetailResponse>>> ChangeStatusAsync(
        [FromRoute] long supportTicketId,
        [FromBody] ChangeSupportTicketStatusRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _sender.Send(
            new ChangeSupportTicketStatusCommand(supportTicketId, request.Status, request.Remarks),
            cancellationToken);

        return Success(response, "Support ticket status updated successfully.");
    }

    [HttpPost("{supportTicketId:long}/change-priority")]
    [Authorize(Policy = PermissionNames.SupportManage)]
    [ProducesResponseType(typeof(ApiResponse<SupportTicketDetailResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<SupportTicketDetailResponse>>> ChangePriorityAsync(
        [FromRoute] long supportTicketId,
        [FromBody] ChangeSupportTicketPriorityRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _sender.Send(
            new ChangeSupportTicketPriorityCommand(supportTicketId, request.PriorityId, request.Remarks),
            cancellationToken);

        return Success(response, "Support ticket priority updated successfully.");
    }

    [HttpPost("{supportTicketId:long}/close")]
    [ProducesResponseType(typeof(ApiResponse<SupportTicketDetailResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<SupportTicketDetailResponse>>> CloseAsync(
        [FromRoute] long supportTicketId,
        [FromBody] SupportTicketActionRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _sender.Send(new CloseSupportTicketCommand(supportTicketId, request.Remarks), cancellationToken);

        return Success(response, "Support ticket closed successfully.");
    }

    [HttpPost("{supportTicketId:long}/reopen")]
    [ProducesResponseType(typeof(ApiResponse<SupportTicketDetailResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<SupportTicketDetailResponse>>> ReopenAsync(
        [FromRoute] long supportTicketId,
        [FromBody] SupportTicketActionRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _sender.Send(new ReopenSupportTicketCommand(supportTicketId, request.Remarks), cancellationToken);

        return Success(response, "Support ticket reopened successfully.");
    }
}
