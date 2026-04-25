using Coolzo.Application.Features.CustomerAuth.Commands.ChangeCustomerPassword;
using Coolzo.Application.Features.CustomerAuth.Commands.ForgotCustomerPassword;
using Coolzo.Application.Features.CustomerAuth.Commands.RegisterCustomer;
using Coolzo.Contracts.Common;
using Coolzo.Contracts.Requests.CustomerAuth;
using Coolzo.Contracts.Responses.CustomerAuth;
using Coolzo.Shared.Constants;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Coolzo.Api.Controllers;

[Route("api/customer-auth")]
public sealed class CustomerAuthController : ApiControllerBase
{
    private readonly ISender _sender;

    public CustomerAuthController(ISender sender)
    {
        _sender = sender;
    }

    [AllowAnonymous]
    [HttpPost("register")]
    [ProducesResponseType(typeof(ApiResponse<CustomerAccountResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<CustomerAccountResponse>>> RegisterAsync(
        [FromBody] RegisterCustomerRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _sender.Send(
            new RegisterCustomerCommand(
                request.CustomerName,
                request.MobileNumber,
                request.EmailAddress,
                request.Password),
            cancellationToken);

        return Success(response, "Customer registration completed successfully.");
    }

    [AllowAnonymous]
    [HttpPost("forgot-password")]
    [ProducesResponseType(typeof(ApiResponse<CustomerPasswordOperationResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<CustomerPasswordOperationResponse>>> ForgotPasswordAsync(
        [FromBody] ForgotCustomerPasswordRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _sender.Send(new ForgotCustomerPasswordCommand(request.LoginId), cancellationToken);

        return Success(response, "Customer password reset completed successfully.");
    }

    [AllowAnonymous]
    [HttpPost("reset-password")]
    [ProducesResponseType(typeof(ApiResponse<CustomerPasswordOperationResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<CustomerPasswordOperationResponse>>> ResetPasswordAsync(
        [FromBody] ForgotCustomerPasswordRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _sender.Send(new ForgotCustomerPasswordCommand(request.LoginId), cancellationToken);

        return Success(response, "Customer password reset completed successfully.");
    }

    [Authorize(Roles = RoleNames.Customer)]
    [HttpPost("change-password")]
    [ProducesResponseType(typeof(ApiResponse<CustomerPasswordOperationResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<CustomerPasswordOperationResponse>>> ChangePasswordAsync(
        [FromBody] ChangeCustomerPasswordRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _sender.Send(
            new ChangeCustomerPasswordCommand(request.CurrentPassword, request.NewPassword),
            cancellationToken);

        return Success(response, "Customer password changed successfully.");
    }
}
