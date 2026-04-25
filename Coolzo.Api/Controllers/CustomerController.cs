using Coolzo.Application.Features.CustomerAccounts.Commands.CreateCustomerAccount;
using Coolzo.Application.Features.CustomerManagement;
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

[Authorize]
[Route("api/customers")]
public sealed class CustomerController : ApiControllerBase
{
    private readonly ISender _sender;

    public CustomerController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet]
    [Authorize(Policy = PermissionNames.UserRead)]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<CustomerAdminListItemResponse>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<PagedResult<CustomerAdminListItemResponse>>>> GetCustomersAsync(
        [FromQuery] string? searchTerm,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var response = await _sender.Send(new GetAdminCustomerListQuery(searchTerm, pageNumber, pageSize), cancellationToken);

        return Success(response);
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

    [HttpGet("{customerId:long}")]
    [Authorize(Policy = PermissionNames.UserRead)]
    [ProducesResponseType(typeof(ApiResponse<CustomerAdminDetailResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<CustomerAdminDetailResponse>>> GetByIdAsync(
        [FromRoute] long customerId,
        CancellationToken cancellationToken)
    {
        var response = await _sender.Send(new GetAdminCustomerDetailQuery(customerId), cancellationToken);

        return Success(response);
    }

    [HttpPut("{customerId:long}")]
    [Authorize(Policy = PermissionNames.UserUpdate)]
    [ProducesResponseType(typeof(ApiResponse<CustomerAdminDetailResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<CustomerAdminDetailResponse>>> UpdateAsync(
        [FromRoute] long customerId,
        [FromBody] UpdateAdminCustomerRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _sender.Send(
            new UpdateAdminCustomerCommand(customerId, request.CustomerName, request.MobileNumber, request.EmailAddress),
            cancellationToken);

        return Success(response, "Customer updated successfully.");
    }

    [HttpGet("{customerId:long}/addresses")]
    [Authorize(Policy = PermissionNames.UserRead)]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyCollection<CustomerAddressResponse>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IReadOnlyCollection<CustomerAddressResponse>>>> GetAddressesAsync(
        [FromRoute] long customerId,
        CancellationToken cancellationToken)
    {
        var response = await _sender.Send(new GetAdminCustomerDetailQuery(customerId), cancellationToken);

        return Success(response.Addresses);
    }

    [HttpPost("{customerId:long}/addresses")]
    [Authorize(Policy = PermissionNames.UserUpdate)]
    [ProducesResponseType(typeof(ApiResponse<CustomerAddressResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<CustomerAddressResponse>>> CreateAddressAsync(
        [FromRoute] long customerId,
        [FromBody] CreateCustomerAddressRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _sender.Send(
            new CreateAdminCustomerAddressCommand(
                customerId,
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

    [HttpPut("{customerId:long}/addresses/{addressId:long}")]
    [Authorize(Policy = PermissionNames.UserUpdate)]
    [ProducesResponseType(typeof(ApiResponse<CustomerAddressResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<CustomerAddressResponse>>> UpdateAddressAsync(
        [FromRoute] long customerId,
        [FromRoute] long addressId,
        [FromBody] UpdateCustomerAddressRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _sender.Send(
            new UpdateAdminCustomerAddressCommand(
                customerId,
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

    [HttpGet("{customerId:long}/equipment")]
    [Authorize(Policy = PermissionNames.UserRead)]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyCollection<CustomerEquipmentResponse>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IReadOnlyCollection<CustomerEquipmentResponse>>>> GetEquipmentAsync(
        [FromRoute] long customerId,
        CancellationToken cancellationToken)
    {
        var response = await _sender.Send(new GetAdminCustomerDetailQuery(customerId), cancellationToken);

        return Success(response.Equipment);
    }

    [HttpPost("{customerId:long}/equipment")]
    [Authorize(Policy = PermissionNames.UserUpdate)]
    [ProducesResponseType(typeof(ApiResponse<CustomerEquipmentResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<CustomerEquipmentResponse>>> CreateEquipmentAsync(
        [FromRoute] long customerId,
        [FromBody] CreateCustomerEquipmentRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _sender.Send(
            new CreateAdminCustomerEquipmentCommand(
                customerId,
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

    [HttpPut("{customerId:long}/equipment/{equipmentId:long}")]
    [Authorize(Policy = PermissionNames.UserUpdate)]
    [ProducesResponseType(typeof(ApiResponse<CustomerEquipmentResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<CustomerEquipmentResponse>>> UpdateEquipmentAsync(
        [FromRoute] long customerId,
        [FromRoute] long equipmentId,
        [FromBody] UpdateCustomerEquipmentRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _sender.Send(
            new UpdateAdminCustomerEquipmentCommand(
                customerId,
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

    [HttpPost("{customerId:long}/notes")]
    [Authorize(Policy = PermissionNames.UserUpdate)]
    [ProducesResponseType(typeof(ApiResponse<CustomerNoteResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<CustomerNoteResponse>>> CreateNoteAsync(
        [FromRoute] long customerId,
        [FromBody] CreateCustomerNoteRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _sender.Send(
            new CreateCustomerNoteCommand(customerId, request.Content, request.IsPrivate, request.NoteType),
            cancellationToken);

        return Success(response, "Customer note added successfully.");
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
