using Coolzo.Application.Features.GapPhaseA.Lead;
using Coolzo.Contracts.Common;
using Coolzo.Contracts.Requests.GapPhaseA;
using Coolzo.Contracts.Responses.GapPhaseA;
using Coolzo.Shared.Constants;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Coolzo.Api.Controllers;

[Route("api/leads")]
public sealed class LeadController : ApiControllerBase
{
    private readonly ISender _sender;

    public LeadController(ISender sender)
    {
        _sender = sender;
    }

    [AllowAnonymous]
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<LeadResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<LeadResponse>>> CreateLeadAsync(
        [FromBody] CreateLeadRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _sender.Send(
            new CreateLeadCommand(
                request.CustomerName,
                request.MobileNumber,
                request.EmailAddress,
                request.SourceChannel,
                request.AddressLine1,
                request.AddressLine2,
                request.CityName,
                request.Pincode,
                request.ServiceId,
                request.AcTypeId,
                request.TonnageId,
                request.BrandId,
                request.SlotAvailabilityId,
                request.InquiryNotes),
            cancellationToken);

        return Success(response, "Lead created successfully.");
    }

    [Authorize(Policy = PermissionNames.ServiceRequestRead)]
    [HttpGet("analytics")]
    [ProducesResponseType(typeof(ApiResponse<LeadAnalyticsResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<LeadAnalyticsResponse>>> GetAnalyticsAsync(
        [FromQuery] DateOnly? fromDate,
        [FromQuery] DateOnly? toDate,
        CancellationToken cancellationToken)
    {
        var response = await _sender.Send(new GetLeadAnalyticsQuery(fromDate, toDate), cancellationToken);

        return Success(response);
    }

    [Authorize(Policy = PermissionNames.ServiceRequestRead)]
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<LeadListItemResponse>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<PagedResult<LeadListItemResponse>>>> GetLeadsAsync(
        [FromQuery] string? searchTerm,
        [FromQuery] string? leadStatus,
        [FromQuery] string? sourceChannel,
        [FromQuery] DateOnly? createdFrom,
        [FromQuery] DateOnly? createdTo,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var response = await _sender.Send(
            new GetLeadListQuery(searchTerm, leadStatus, sourceChannel, createdFrom, createdTo, pageNumber, pageSize),
            cancellationToken);

        return Success(response);
    }

    [Authorize(Policy = PermissionNames.ServiceRequestRead)]
    [HttpGet("{leadId:long}")]
    [ProducesResponseType(typeof(ApiResponse<LeadDetailResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<LeadDetailResponse>>> GetLeadByIdAsync(
        [FromRoute] long leadId,
        CancellationToken cancellationToken)
    {
        var response = await _sender.Send(new GetLeadDetailQuery(leadId), cancellationToken);

        return Success(response);
    }

    [Authorize(Policy = PermissionNames.ServiceRequestUpdate)]
    [HttpPut("{leadId:long}/assign")]
    [ProducesResponseType(typeof(ApiResponse<LeadResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<LeadResponse>>> AssignLeadAsync(
        [FromRoute] long leadId,
        [FromBody] AssignLeadRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _sender.Send(new AssignLeadCommand(leadId, request.AssignedUserId, request.Remarks), cancellationToken);

        return Success(response, "Lead assigned successfully.");
    }

    [Authorize(Policy = PermissionNames.ServiceRequestUpdate)]
    [HttpPut("{leadId:long}/status")]
    [ProducesResponseType(typeof(ApiResponse<LeadResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<LeadResponse>>> UpdateLeadStatusAsync(
        [FromRoute] long leadId,
        [FromBody] UpdateLeadStatusRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _sender.Send(
            new UpdateLeadStatusCommand(leadId, request.LeadStatus, request.Remarks, request.LostReason),
            cancellationToken);

        return Success(response, "Lead status updated successfully.");
    }

    [Authorize(Policy = PermissionNames.BookingCreate)]
    [HttpPost("{leadId:long}/convert-to-booking")]
    [ProducesResponseType(typeof(ApiResponse<LeadResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<LeadResponse>>> ConvertToBookingAsync(
        [FromRoute] long leadId,
        [FromBody] ConvertLeadToBookingRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _sender.Send(
            new ConvertLeadToBookingCommand(
                leadId,
                request.ServiceId,
                request.AcTypeId,
                request.TonnageId,
                request.BrandId,
                request.SlotAvailabilityId,
                request.AddressLine1,
                request.AddressLine2,
                request.CityName,
                request.Pincode,
                request.InquiryNotes),
            cancellationToken);

        return Success(response, "Lead converted to booking successfully.");
    }

    [Authorize(Policy = PermissionNames.ServiceRequestCreate)]
    [HttpPost("{leadId:long}/convert-to-sr")]
    [ProducesResponseType(typeof(ApiResponse<LeadResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<LeadResponse>>> ConvertToServiceRequestAsync(
        [FromRoute] long leadId,
        [FromBody] ConvertLeadToServiceRequestRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _sender.Send(
            new ConvertLeadToServiceRequestCommand(
                leadId,
                request.ServiceId,
                request.AcTypeId,
                request.TonnageId,
                request.BrandId,
                request.SlotAvailabilityId,
                request.AddressLine1,
                request.AddressLine2,
                request.CityName,
                request.Pincode,
                request.InquiryNotes),
            cancellationToken);

        return Success(response, "Lead converted to service request successfully.");
    }

    [Authorize(Policy = PermissionNames.ServiceRequestUpdate)]
    [HttpPost("{leadId:long}/notes")]
    [ProducesResponseType(typeof(ApiResponse<LeadResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<LeadResponse>>> AddNoteAsync(
        [FromRoute] long leadId,
        [FromBody] AddLeadNoteRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _sender.Send(new AddLeadNoteCommand(leadId, request.NoteText, request.IsInternal), cancellationToken);

        return Success(response, "Lead note added successfully.");
    }
}
