using Coolzo.Application.Features.ServiceCatalogAdmin.Commands.SetServiceImage;
using Coolzo.Contracts.Common;
using Coolzo.Contracts.Requests.Admin;
using Coolzo.Contracts.Responses.Booking;
using Coolzo.Shared.Constants;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Coolzo.Api.Controllers;

[Authorize]
[Route("api/admin/services")]
public sealed class ServiceCatalogAdminController : ApiControllerBase
{
    private readonly ISender _sender;

    public ServiceCatalogAdminController(ISender sender)
    {
        _sender = sender;
    }

    [HttpPut("{serviceId:long}/image")]
    [Authorize(Policy = PermissionNames.LookupManage)]
    public async Task<ActionResult<ApiResponse<ServiceLookupResponse>>> SetImageAsync(
        [FromRoute] long serviceId,
        [FromBody] SetServiceImageRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _sender.Send(
            new SetServiceImageCommand(serviceId, request.ImageUrl),
            cancellationToken);

        return Success(response, "Service image updated successfully.");
    }
}
