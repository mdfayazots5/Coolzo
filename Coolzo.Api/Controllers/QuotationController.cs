using Coolzo.Application.Features.Billing.Commands.ApproveQuotation;
using Coolzo.Application.Features.Billing.Commands.CreateQuotationFromJob;
using Coolzo.Application.Features.Billing.Commands.RejectQuotation;
using Coolzo.Application.Features.Billing.Queries.GetQuotationById;
using Coolzo.Application.Features.Billing.Queries.GetQuotationByJob;
using Coolzo.Application.Features.Billing.Queries.SearchQuotations;
using Coolzo.Contracts.Common;
using Coolzo.Contracts.Requests.Billing;
using Coolzo.Contracts.Responses.Billing;
using Coolzo.Shared.Constants;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Coolzo.Api.Controllers;

[Route("api/quotations")]
public sealed class QuotationController : ApiControllerBase
{
    private readonly ISender _sender;

    public QuotationController(ISender sender)
    {
        _sender = sender;
    }

    [Authorize(Roles = RoleNames.Technician)]
    [HttpPost("from-job/{jobCardId:long}")]
    [ProducesResponseType(typeof(ApiResponse<QuotationDetailResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<QuotationDetailResponse>>> CreateFromJobAsync(
        [FromRoute] long jobCardId,
        [FromBody] CreateQuotationFromJobRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _sender.Send(
            new CreateQuotationFromJobCommand(jobCardId, request.Lines, request.DiscountAmount, request.TaxPercentage, request.Remarks),
            cancellationToken);

        return Success(response, "Quotation submitted for customer approval.");
    }

    [Authorize(Policy = PermissionNames.QuotationRead)]
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<QuotationListItemResponse>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<PagedResult<QuotationListItemResponse>>>> SearchAsync(
        [FromQuery] string? status,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var response = await _sender.Send(new SearchQuotationsQuery(status, pageNumber, pageSize), cancellationToken);

        return Success(response);
    }

    [Authorize]
    [HttpGet("{id:long}")]
    [ProducesResponseType(typeof(ApiResponse<QuotationDetailResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<QuotationDetailResponse>>> GetByIdAsync(
        [FromRoute] long id,
        CancellationToken cancellationToken)
    {
        var response = await _sender.Send(new GetQuotationByIdQuery(id), cancellationToken);

        return Success(response);
    }

    [Authorize]
    [HttpGet("job/{jobCardId:long}")]
    [ProducesResponseType(typeof(ApiResponse<QuotationDetailResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<QuotationDetailResponse>>> GetByJobCardAsync(
        [FromRoute] long jobCardId,
        CancellationToken cancellationToken)
    {
        var response = await _sender.Send(new GetQuotationByJobQuery(jobCardId), cancellationToken);

        return Success(response);
    }

    [Authorize]
    [HttpPost("{id:long}/approve")]
    [ProducesResponseType(typeof(ApiResponse<QuotationDetailResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<QuotationDetailResponse>>> ApproveAsync(
        [FromRoute] long id,
        [FromBody] QuotationDecisionRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _sender.Send(new ApproveQuotationCommand(id, request.Remarks), cancellationToken);

        return Success(response, "Quotation approved successfully.");
    }

    [Authorize]
    [HttpPost("{id:long}/reject")]
    [ProducesResponseType(typeof(ApiResponse<QuotationDetailResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<QuotationDetailResponse>>> RejectAsync(
        [FromRoute] long id,
        [FromBody] QuotationDecisionRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _sender.Send(new RejectQuotationCommand(id, request.Remarks), cancellationToken);

        return Success(response, "Quotation rejected successfully.");
    }
}
