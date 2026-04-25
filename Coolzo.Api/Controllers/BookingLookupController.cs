using Coolzo.Application.Features.BookingLookup.Queries.GetAcTypes;
using Coolzo.Application.Features.BookingLookup.Queries.GetAvailableSlots;
using Coolzo.Application.Features.BookingLookup.Queries.GetBrands;
using Coolzo.Application.Features.BookingLookup.Queries.GetServiceCategories;
using Coolzo.Application.Features.BookingLookup.Queries.GetServices;
using Coolzo.Application.Features.BookingLookup.Queries.GetTonnages;
using Coolzo.Application.Features.BookingLookup.Queries.GetZones;
using Coolzo.Application.Features.BookingLookup.Queries.GetZoneByPincode;
using Coolzo.Contracts.Common;
using Coolzo.Contracts.Responses.Booking;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Coolzo.Api.Controllers;

[Route("api/booking-lookups")]
public sealed class BookingLookupController : ApiControllerBase
{
    private readonly ISender _sender;

    public BookingLookupController(ISender sender)
    {
        _sender = sender;
    }

    [AllowAnonymous]
    [HttpGet("service-categories")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyCollection<ServiceCategoryLookupResponse>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IReadOnlyCollection<ServiceCategoryLookupResponse>>>> GetServiceCategoriesAsync(
        [FromQuery] string? search,
        CancellationToken cancellationToken)
    {
        var response = await _sender.Send(new GetServiceCategoriesQuery(search), cancellationToken);

        return Success(response);
    }

    [AllowAnonymous]
    [HttpGet("services")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyCollection<ServiceLookupResponse>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IReadOnlyCollection<ServiceLookupResponse>>>> GetServicesAsync(
        [FromQuery] long? serviceCategoryId,
        [FromQuery] string? search,
        CancellationToken cancellationToken)
    {
        var response = await _sender.Send(new GetServicesQuery(serviceCategoryId, search), cancellationToken);

        return Success(response);
    }

    [AllowAnonymous]
    [HttpGet("ac-types")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyCollection<AcTypeLookupResponse>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IReadOnlyCollection<AcTypeLookupResponse>>>> GetAcTypesAsync(
        [FromQuery] string? search,
        CancellationToken cancellationToken)
    {
        var response = await _sender.Send(new GetAcTypesQuery(search), cancellationToken);

        return Success(response);
    }

    [AllowAnonymous]
    [HttpGet("tonnage")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyCollection<TonnageLookupResponse>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IReadOnlyCollection<TonnageLookupResponse>>>> GetTonnagesAsync(
        [FromQuery] string? search,
        CancellationToken cancellationToken)
    {
        var response = await _sender.Send(new GetTonnagesQuery(search), cancellationToken);

        return Success(response);
    }

    [AllowAnonymous]
    [HttpGet("brands")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyCollection<BrandLookupResponse>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IReadOnlyCollection<BrandLookupResponse>>>> GetBrandsAsync(
        [FromQuery] string? search,
        CancellationToken cancellationToken)
    {
        var response = await _sender.Send(new GetBrandsQuery(search), cancellationToken);

        return Success(response);
    }

    [AllowAnonymous]
    [HttpGet("zones")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyCollection<ZoneListItemResponse>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IReadOnlyCollection<ZoneListItemResponse>>>> GetZonesAsync(
        [FromQuery] string? search,
        CancellationToken cancellationToken)
    {
        var response = await _sender.Send(new GetZonesQuery(search), cancellationToken);

        return Success(response);
    }

    [AllowAnonymous]
    [HttpGet("zones/by-pincode/{pincode}")]
    [ProducesResponseType(typeof(ApiResponse<ZoneLookupResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<ZoneLookupResponse>>> GetZoneByPincodeAsync(
        [FromRoute] string pincode,
        CancellationToken cancellationToken)
    {
        var response = await _sender.Send(new GetZoneByPincodeQuery(pincode), cancellationToken);

        return Success(response);
    }

    [AllowAnonymous]
    [HttpGet("slots")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyCollection<SlotAvailabilityResponse>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IReadOnlyCollection<SlotAvailabilityResponse>>>> GetSlotsAsync(
        [FromQuery] long zoneId,
        [FromQuery] DateOnly slotDate,
        CancellationToken cancellationToken)
    {
        var response = await _sender.Send(new GetAvailableSlotsQuery(zoneId, slotDate), cancellationToken);

        return Success(response);
    }
}
