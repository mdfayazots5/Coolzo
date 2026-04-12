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
[Route("api/v{version:apiVersion}/customers")]
public sealed class CustomerAddressController : ApiControllerBase
{
    private readonly ISender _sender;

    public CustomerAddressController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet("me/addresses")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyCollection<CustomerAddressResponse>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IReadOnlyCollection<CustomerAddressResponse>>>> GetMyAddressesAsync(CancellationToken cancellationToken)
    {
        var response = await _sender.Send(new GetMyCustomerAddressesQuery(), cancellationToken);
        return Success(response);
    }

    [HttpPost("me/addresses")]
    [ProducesResponseType(typeof(ApiResponse<CustomerAddressResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<CustomerAddressResponse>>> CreateAddressAsync([FromBody] CreateCustomerAddressRequest request, CancellationToken cancellationToken)
    {
        var response = await _sender.Send(
            new CreateMyCustomerAddressCommand(
                request.AddressLabel,
                request.AddressLine1,
                request.AddressLine2,
                request.Landmark,
                request.CityName,
                request.Pincode,
                request.ZoneId,
                request.Latitude,
                request.Longitude,
                request.IsDefault,
                request.StateName,
                request.AddressType),
            cancellationToken);

        return Success(response, "Customer address created successfully.");
    }

    [HttpPut("me/addresses/{addressId:long}")]
    [ProducesResponseType(typeof(ApiResponse<CustomerAddressResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<CustomerAddressResponse>>> UpdateAddressAsync([FromRoute] long addressId, [FromBody] UpdateCustomerAddressRequest request, CancellationToken cancellationToken)
    {
        var response = await _sender.Send(
            new UpdateMyCustomerAddressCommand(
                addressId,
                request.AddressLabel,
                request.AddressLine1,
                request.AddressLine2,
                request.Landmark,
                request.CityName,
                request.Pincode,
                request.ZoneId,
                request.Latitude,
                request.Longitude,
                request.IsDefault,
                request.StateName,
                request.AddressType),
            cancellationToken);

        return Success(response, "Customer address updated successfully.");
    }

    [HttpDelete("me/addresses/{addressId:long}")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<object>>> DeleteAddressAsync([FromRoute] long addressId, CancellationToken cancellationToken)
    {
        await _sender.Send(new DeleteMyCustomerAddressCommand(addressId), cancellationToken);
        return Success<object>(new { addressId }, "Customer address deleted successfully.");
    }
}
