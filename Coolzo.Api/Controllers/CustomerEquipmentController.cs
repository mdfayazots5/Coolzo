using Asp.Versioning;
using Coolzo.Application.Features.CustomerApp;
using Coolzo.Contracts.Common;
using Coolzo.Contracts.Requests.Customer;
using Coolzo.Contracts.Responses.Customer;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Coolzo.Api.Controllers;

[ApiVersion("1.0")]
[Authorize]
[Route("api/v{version:apiVersion}/customers/me/equipment")]
public sealed class CustomerEquipmentController : ApiControllerBase
{
    private readonly ISender _sender;

    public CustomerEquipmentController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyCollection<CustomerEquipmentResponse>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IReadOnlyCollection<CustomerEquipmentResponse>>>> GetMyEquipmentAsync(CancellationToken cancellationToken)
    {
        var response = await _sender.Send(new GetMyCustomerEquipmentQuery(), cancellationToken);
        return Success(response);
    }

    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<CustomerEquipmentResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<CustomerEquipmentResponse>>> CreateEquipmentAsync(
        [FromBody] CreateCustomerEquipmentRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _sender.Send(
            new CreateMyCustomerEquipmentCommand(
                request.Name,
                request.Type,
                request.Brand,
                request.Capacity,
                request.Location,
                request.PurchaseDate,
                request.LastServiceDate,
                request.SerialNumber),
            cancellationToken);

        return Success(response, "Customer equipment created successfully.");
    }

    [HttpPut("{equipmentId:long}")]
    [ProducesResponseType(typeof(ApiResponse<CustomerEquipmentResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<CustomerEquipmentResponse>>> UpdateEquipmentAsync(
        [FromRoute] long equipmentId,
        [FromBody] UpdateCustomerEquipmentRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _sender.Send(
            new UpdateMyCustomerEquipmentCommand(
                equipmentId,
                request.Name,
                request.Type,
                request.Brand,
                request.Capacity,
                request.Location,
                request.PurchaseDate,
                request.LastServiceDate,
                request.SerialNumber),
            cancellationToken);

        return Success(response, "Customer equipment updated successfully.");
    }

    [HttpDelete("{equipmentId:long}")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<object>>> DeleteEquipmentAsync(
        [FromRoute] long equipmentId,
        CancellationToken cancellationToken)
    {
        await _sender.Send(new DeleteMyCustomerEquipmentCommand(equipmentId), cancellationToken);
        return Success<object>(new { equipmentId }, "Customer equipment deleted successfully.");
    }
}
