using Asp.Versioning;
using Coolzo.Api.Mapping;
using Coolzo.Application.Features.CustomerApp;
using Coolzo.Application.Features.Auth.Commands.AuthSession;
using Coolzo.Application.Features.CustomerAuth.Commands.ChangeCustomerPassword;
using Coolzo.Application.Features.CustomerAuth.Commands.ResetCustomerPasswordWithOtp;
using Coolzo.Application.Features.Auth.Commands.Login;
using Coolzo.Application.Features.Auth.Commands.RefreshToken;
using Coolzo.Application.Features.Auth.Queries.GetCurrentUser;
using Coolzo.Contracts.Common;
using Coolzo.Contracts.Requests.Auth;
using Coolzo.Contracts.Requests.CustomerAuth;
using Coolzo.Contracts.Responses.Auth;
using Coolzo.Contracts.Responses.Customer;
using Coolzo.Shared.Constants;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Coolzo.Api.Controllers;

[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/auth")]
public sealed class AuthController : ApiControllerBase
{
    private readonly ISender _sender;

    public AuthController(ISender sender)
    {
        _sender = sender;
    }

    [AllowAnonymous]
    [HttpPost("otp/send")]
    [ProducesResponseType(typeof(ApiResponse<AuthActionResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<AuthActionResponse>>> SendOtpAsync(
        [FromBody] SendCustomerOtpRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _sender.Send(new SendOtpCommand(request.Phone), cancellationToken);

        return Success(response, response.Message);
    }

    [AllowAnonymous]
    [HttpPost("otp/verify")]
    [ProducesResponseType(typeof(ApiResponse<AuthTokenResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<AuthTokenResponse>>> VerifyCustomerOtpAsync(
        [FromBody] VerifyCustomerOtpRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _sender.Send(new VerifyOtpCommand(request.Phone, request.Otp), cancellationToken);

        return Success(response, "OTP verified successfully.");
    }

    [AllowAnonymous]
    [HttpPost("login")]
    [ProducesResponseType(typeof(ApiResponse<AuthTokenResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<AuthTokenResponse>>> LoginAsync([FromBody] LoginRequest request, CancellationToken cancellationToken)
    {
        var response = await _sender.Send(new LoginCommand(request.UserNameOrEmail, request.Password), cancellationToken);

        return Success(response, "Login completed successfully.");
    }

    [AllowAnonymous]
    [HttpPost("refresh")]
    [ProducesResponseType(typeof(ApiResponse<AuthTokenResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<AuthTokenResponse>>> RefreshAsync([FromBody] RefreshTokenRequest request, CancellationToken cancellationToken)
    {
        var response = await _sender.Send(new RefreshTokenCommand(request.AccessToken, request.RefreshToken), cancellationToken);

        return Success(response, "Token refreshed successfully.");
    }

    [AllowAnonymous]
    [HttpPost("login-field")]
    [ProducesResponseType(typeof(ApiResponse<AuthTokenResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<AuthTokenResponse>>> LoginFieldAsync(
        [FromBody] LoginFieldRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _sender.Send(new LoginFieldCommand(request.EmployeeId, request.Pin), cancellationToken);

        return Success(response, "Field login completed successfully.");
    }

    [AllowAnonymous]
    [HttpPost("login-otp")]
    [ProducesResponseType(typeof(ApiResponse<AuthTokenResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<AuthTokenResponse>>> LoginOtpAsync(
        [FromBody] LoginOtpRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _sender.Send(new LoginOtpCommand(request.LoginId, request.Otp), cancellationToken);

        return Success(response, "OTP login completed successfully.");
    }

    [AllowAnonymous]
    [HttpPost("verify-otp")]
    [ProducesResponseType(typeof(ApiResponse<AuthTokenResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<AuthTokenResponse>>> VerifyOtpAsync(
        [FromBody] VerifyOtpRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _sender.Send(new VerifyOtpCommand(request.Email, request.Otp), cancellationToken);

        return Success(response, "OTP verified successfully.");
    }

    [AllowAnonymous]
    [HttpPost("refresh-token")]
    [ProducesResponseType(typeof(ApiResponse<AuthTokenResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<AuthTokenResponse>>> RefreshTokenAsync(
        [FromBody] RefreshTokenRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _sender.Send(new RefreshTokenCommand(request.AccessToken, request.RefreshToken), cancellationToken);

        return Success(response, "Token refreshed successfully.");
    }

    [AllowAnonymous]
    [HttpPost("forgot-password")]
    [ProducesResponseType(typeof(ApiResponse<AuthActionResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<AuthActionResponse>>> ForgotPasswordAsync(
        [FromBody] ForgotPasswordRequest request,
        CancellationToken cancellationToken)
    {
        var loginId = request.LoginId ?? request.Phone;

        if (!string.IsNullOrWhiteSpace(loginId))
        {
            var otpResponse = await _sender.Send(new SendOtpCommand(loginId), cancellationToken);
            return Success(otpResponse, otpResponse.Message);
        }

        var response = await _sender.Send(new ForgotPasswordCommand(request.Email ?? string.Empty), cancellationToken);

        return Success(response, response.Message);
    }

    [AllowAnonymous]
    [HttpPost("reset-password")]
    [ProducesResponseType(typeof(ApiResponse<AuthActionResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<AuthActionResponse>>> ResetPasswordAsync(
        [FromBody] ResetPasswordRequest request,
        CancellationToken cancellationToken)
    {
        if (!string.IsNullOrWhiteSpace(request.Phone) &&
            !string.IsNullOrWhiteSpace(request.Otp) &&
            !string.IsNullOrWhiteSpace(request.NewPassword))
        {
            var customerResponse = await _sender.Send(
                new ResetCustomerPasswordWithOtpCommand(request.Phone, request.Otp, request.NewPassword),
                cancellationToken);

            return Success(
                new AuthActionResponse(customerResponse.PasswordUpdated, "Customer password reset successfully."),
                "Customer password reset successfully.");
        }

        var response = await _sender.Send(new ResetPasswordCommand(request.Token ?? string.Empty, request.Password ?? string.Empty), cancellationToken);

        return Success(response, response.Message);
    }

    [Authorize(Roles = RoleNames.Customer)]
    [HttpPost("change-password")]
    [ProducesResponseType(typeof(ApiResponse<AuthActionResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<AuthActionResponse>>> ChangePasswordAsync(
        [FromBody] ChangeCustomerPasswordRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _sender.Send(
            new ChangeCustomerPasswordCommand(request.CurrentPassword, request.NewPassword),
            cancellationToken);

        return Success(
            new AuthActionResponse(response.PasswordUpdated, "Customer password changed successfully."),
            "Customer password changed successfully.");
    }

    [Authorize]
    [HttpPost("logout")]
    [ProducesResponseType(typeof(ApiResponse<AuthActionResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<AuthActionResponse>>> LogoutAsync(
        [FromBody] LogoutRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _sender.Send(new LogoutCommand(request.RefreshToken), cancellationToken);

        return Success(response, response.Message);
    }

    [Authorize(Roles = RoleNames.Customer)]
    [HttpDelete("account")]
    [ProducesResponseType(typeof(ApiResponse<CustomerAccountDeletionResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<CustomerAccountDeletionResponse>>> DeleteCustomerAccountAsync(
        CancellationToken cancellationToken)
    {
        var response = await _sender.Send(new DeactivateMyCustomerAccountCommand("Customer app self-service deletion"), cancellationToken);

        return Success(response, "Customer account deactivated successfully.");
    }

    [Authorize]
    [HttpGet("me")]
    [ProducesResponseType(typeof(ApiResponse<CurrentUserResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<CurrentUserResponse>>> GetCurrentUserAsync(CancellationToken cancellationToken)
    {
        var response = await _sender.Send(new GetCurrentUserQuery(), cancellationToken);

        return Success(response);
    }

    [Authorize]
    [HttpGet("me/permissions")]
    [ProducesResponseType(typeof(ApiResponse<AuthPermissionSnapshotResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<AuthPermissionSnapshotResponse>>> GetPermissionSnapshotAsync(CancellationToken cancellationToken)
    {
        var response = await _sender.Send(new GetCurrentUserQuery(), cancellationToken);
        var modules = PermissionModuleMatrixMapper.Build(response.Permissions, response.Roles);
        var dataScope = PermissionDataScopeMapper.Resolve(response.Roles);

        return Success(new AuthPermissionSnapshotResponse(modules, dataScope, response.Permissions));
    }

    [Authorize(Roles = RoleNames.SuperAdmin)]
    [HttpPost("force-logout/{userId:long}")]
    [ProducesResponseType(typeof(ApiResponse<AuthActionResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<AuthActionResponse>>> ForceLogoutAsync(
        [FromRoute] long userId,
        CancellationToken cancellationToken)
    {
        var response = await _sender.Send(new ForceLogoutCommand(userId), cancellationToken);

        return Success(response, response.Message);
    }
}
