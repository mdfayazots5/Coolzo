using Asp.Versioning;
using Coolzo.Application.Features.CustomerAccounts.Commands.CreateCustomerAccount;
using Coolzo.Application.Features.CustomerAccounts.Commands.ResetCustomerPassword;
using Coolzo.Application.Features.CustomerApp;
using Coolzo.Contracts.Common;
using Coolzo.Contracts.Requests.Customer;
using Coolzo.Contracts.Responses.Customer;
using Coolzo.Contracts.Responses.CustomerAuth;
using Coolzo.Shared.Constants;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Coolzo.Api.Controllers;

[ApiVersion("1.0")]
[Authorize]
[Route("api/v{version:apiVersion}/customers")]
public sealed class CustomerController : ApiControllerBase
{
    private readonly ISender _sender;

    public CustomerController(ISender sender)
    {
        _sender = sender;
    }

    [HttpPost]
    [Authorize(Policy = PermissionNames.UserCreate)]
    [ProducesResponseType(typeof(ApiResponse<CustomerAccountResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<CustomerAccountResponse>>> CreateAsync(
        [FromBody] CreateCustomerAccountRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _sender.Send(
            new CreateCustomerAccountCommand(request.CustomerName, request.MobileNumber, request.EmailAddress),
            cancellationToken);

        return Success(response, "Customer account created successfully.");
    }

    [HttpPost("{customerId:long}/reset-password")]
    [Authorize(Policy = PermissionNames.UserUpdate)]
    [ProducesResponseType(typeof(ApiResponse<CustomerPasswordOperationResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<CustomerPasswordOperationResponse>>> ResetPasswordAsync(
        [FromRoute] long customerId,
        [FromBody] ResetCustomerPasswordRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _sender.Send(new ResetCustomerPasswordCommand(customerId, request.Reason), cancellationToken);

        return Success(response, "Customer password reset successfully.");
    }

    [HttpGet("me/profile")]
    [Authorize(Roles = RoleNames.Customer)]
    [ProducesResponseType(typeof(ApiResponse<CustomerProfileResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<CustomerProfileResponse>>> GetMyProfileAsync(CancellationToken cancellationToken)
    {
        var response = await _sender.Send(new GetMyCustomerProfileQuery(), cancellationToken);
        return Success(response);
    }

    [HttpPut("me/profile")]
    [Authorize(Roles = RoleNames.Customer)]
    [ProducesResponseType(typeof(ApiResponse<CustomerProfileResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<CustomerProfileResponse>>> UpdateMyProfileAsync(
        [FromBody] UpdateCustomerProfileRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _sender.Send(
            new UpdateMyCustomerProfileCommand(
                request.CustomerName,
                request.MobileNumber,
                request.EmailAddress,
                request.PhotoUrl,
                request.MembershipStatus),
            cancellationToken);

        return Success(response, "Customer profile updated successfully.");
    }

    [HttpPost("me/deactivate")]
    [Authorize(Roles = RoleNames.Customer)]
    [ProducesResponseType(typeof(ApiResponse<CustomerAccountDeletionResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<CustomerAccountDeletionResponse>>> DeactivateMyAccountAsync(
        [FromBody] DeleteCustomerAccountRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _sender.Send(new DeactivateMyCustomerAccountCommand(request.Reason), cancellationToken);
        return Success(response, "Customer account deactivated successfully.");
    }
}
