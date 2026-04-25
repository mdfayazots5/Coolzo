using Coolzo.Application.Common.Interfaces;
using Coolzo.Application.Features.CMS.Queries.GetPublicFAQContent;
using Coolzo.Contracts.Common;
using Coolzo.Contracts.Responses.Booking;
using Coolzo.Shared.Constants;
using Coolzo.Shared.Exceptions;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Coolzo.Api.Controllers;

[Route("api/service-types")]
public sealed class ServiceTypesController : ApiControllerBase
{
    private readonly IBookingLookupRepository _bookingLookupRepository;
    private readonly ISender _sender;

    public ServiceTypesController(IBookingLookupRepository bookingLookupRepository, ISender sender)
    {
        _bookingLookupRepository = bookingLookupRepository;
        _sender = sender;
    }

    [AllowAnonymous]
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<ServiceTypeListItemResponse[]>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<ServiceTypeListItemResponse[]>>> GetAllAsync(
        [FromQuery] string? visibility,
        [FromQuery] string? search,
        CancellationToken cancellationToken)
    {
        var services = await _bookingLookupRepository.ListServicesAsync(null, search, cancellationToken);
        var response = services
            .Select(MapListItem)
            .ToArray();

        return Success(response, string.Equals(visibility, "public", StringComparison.OrdinalIgnoreCase)
            ? "Public service types returned successfully."
            : "Service types returned successfully.");
    }

    [AllowAnonymous]
    [HttpGet("{serviceTypeId:long}")]
    [ProducesResponseType(typeof(ApiResponse<ServiceTypeDetailResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<ServiceTypeDetailResponse>>> GetByIdAsync(
        [FromRoute] long serviceTypeId,
        CancellationToken cancellationToken)
    {
        var service = await _bookingLookupRepository.GetServiceByIdAsync(serviceTypeId, cancellationToken)
            ?? throw new AppException(ErrorCodes.NotFound, "The requested service type could not be found.", 404);
        var faqs = await _sender.Send(new GetPublicFAQContentQuery(), cancellationToken);

        return Success(MapDetail(service, faqs));
    }

    [AllowAnonymous]
    [HttpGet("{serviceTypeId:long}/sub-types")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyCollection<ServiceTypeSubTypeResponse>>), StatusCodes.Status200OK)]
    public ActionResult<ApiResponse<IReadOnlyCollection<ServiceTypeSubTypeResponse>>> GetSubTypesAsync([FromRoute] long serviceTypeId)
    {
        return Success<IReadOnlyCollection<ServiceTypeSubTypeResponse>>(Array.Empty<ServiceTypeSubTypeResponse>());
    }

    private static ServiceTypeListItemResponse MapListItem(Coolzo.Domain.Entities.Service service)
    {
        return new ServiceTypeListItemResponse(
            service.ServiceId,
            service.ServiceName,
            service.Summary,
            service.ServiceCategory?.CategoryName ?? string.Empty,
            service.BasePrice,
            service.EstimatedDurationInMinutes,
            ResolveIconKey(service.ServiceCategory?.CategoryName, service.ServiceName));
    }

    private static ServiceTypeDetailResponse MapDetail(
        Coolzo.Domain.Entities.Service service,
        IReadOnlyCollection<Coolzo.Contracts.Responses.Admin.CMSFaqResponse> faqs)
    {
        return new ServiceTypeDetailResponse(
            service.ServiceId,
            service.ServiceName,
            service.Summary,
            service.ServiceCategory?.CategoryName ?? string.Empty,
            service.BasePrice,
            service.EstimatedDurationInMinutes,
            ResolveIconKey(service.ServiceCategory?.CategoryName, service.ServiceName),
            Array.Empty<ServiceTypeSubTypeResponse>(),
            faqs);
    }

    private static string ResolveIconKey(string? categoryName, string serviceName)
    {
        var key = $"{categoryName} {serviceName}".ToLowerInvariant();

        if (key.Contains("repair"))
        {
            return "repair";
        }

        if (key.Contains("clean"))
        {
            return "cleaning";
        }

        if (key.Contains("install"))
        {
            return "installation";
        }

        if (key.Contains("gas"))
        {
            return "gas-refill";
        }

        if (key.Contains("amc"))
        {
            return "amc";
        }

        return "service";
    }
}
