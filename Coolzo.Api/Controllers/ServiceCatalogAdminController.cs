using Coolzo.Application.Features.ServiceCatalogAdmin.Commands.CreateService;
using Coolzo.Application.Features.ServiceCatalogAdmin.Commands.CreateServiceCategory;
using Coolzo.Application.Features.ServiceCatalogAdmin.Commands.DeleteService;
using Coolzo.Application.Features.ServiceCatalogAdmin.Commands.DeleteServiceCategory;
using Coolzo.Application.Features.ServiceCatalogAdmin.Commands.SetServiceImage;
using Coolzo.Application.Features.ServiceCatalogAdmin.Commands.SetServicePrompt;
using Coolzo.Application.Features.ServiceCatalogAdmin.Commands.UpdateService;
using Coolzo.Application.Features.ServiceCatalogAdmin.Commands.UpdateServiceCategory;
using Coolzo.Application.Features.ServiceCatalogAdmin.Queries.GetServiceCatalogAdmin;
using Coolzo.Contracts.Common;
using Coolzo.Contracts.Requests.Admin;
using Coolzo.Contracts.Responses.Admin;
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

    // ── Read: full admin catalog (categories + services + pricing models, incl. inactive) ────────
    [HttpGet("catalog")]
    [Authorize(Policy = PermissionNames.LookupManage)]
    public async Task<ActionResult<ApiResponse<ServiceCatalogAdminResponse>>> GetCatalogAsync(CancellationToken cancellationToken)
    {
        var response = await _sender.Send(new GetServiceCatalogAdminQuery(), cancellationToken);
        return Success(response);
    }

    // ── Service Categories ───────────────────────────────────────────────────────────────────────
    [HttpPost("categories")]
    [Authorize(Policy = PermissionNames.LookupManage)]
    public async Task<ActionResult<ApiResponse<ServiceCategoryAdminResponse>>> CreateCategoryAsync(
        [FromBody] ServiceCategoryUpsertRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _sender.Send(
            new CreateServiceCategoryCommand(request.CategoryName, request.CategoryCode, request.Description, request.ImageUrl, request.ImageAIPrompt, request.IsActive, request.SortOrder),
            cancellationToken);

        return Success(response, "Service category created successfully.");
    }

    [HttpPut("categories/{serviceCategoryId:long}")]
    [Authorize(Policy = PermissionNames.LookupManage)]
    public async Task<ActionResult<ApiResponse<ServiceCategoryAdminResponse>>> UpdateCategoryAsync(
        [FromRoute] long serviceCategoryId,
        [FromBody] ServiceCategoryUpsertRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _sender.Send(
            new UpdateServiceCategoryCommand(serviceCategoryId, request.CategoryName, request.CategoryCode, request.Description, request.ImageUrl, request.ImageAIPrompt, request.IsActive, request.SortOrder),
            cancellationToken);

        return Success(response, "Service category updated successfully.");
    }

    [HttpDelete("categories/{serviceCategoryId:long}")]
    [Authorize(Policy = PermissionNames.LookupManage)]
    public async Task<ActionResult<ApiResponse<bool>>> DeleteCategoryAsync(
        [FromRoute] long serviceCategoryId,
        CancellationToken cancellationToken)
    {
        await _sender.Send(new DeleteServiceCategoryCommand(serviceCategoryId), cancellationToken);
        return Success(true, "Service category deleted successfully.");
    }

    // ── Services ─────────────────────────────────────────────────────────────────────────────────
    [HttpPost]
    [Authorize(Policy = PermissionNames.LookupManage)]
    public async Task<ActionResult<ApiResponse<ServiceAdminResponse>>> CreateServiceAsync(
        [FromBody] ServiceUpsertRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _sender.Send(
            new CreateServiceCommand(
                request.ServiceCategoryId, request.PricingModelId, request.ServiceName, request.ServiceCode,
                request.Summary, request.BasePrice, request.EstimatedDurationInMinutes, request.ImageUrl, request.ImageAIPrompt, request.IsActive, request.SortOrder),
            cancellationToken);

        return Success(response, "Service created successfully.");
    }

    [HttpPut("{serviceId:long}")]
    [Authorize(Policy = PermissionNames.LookupManage)]
    public async Task<ActionResult<ApiResponse<ServiceAdminResponse>>> UpdateServiceAsync(
        [FromRoute] long serviceId,
        [FromBody] ServiceUpsertRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _sender.Send(
            new UpdateServiceCommand(
                serviceId, request.ServiceCategoryId, request.PricingModelId, request.ServiceName, request.ServiceCode,
                request.Summary, request.BasePrice, request.EstimatedDurationInMinutes, request.ImageUrl, request.ImageAIPrompt, request.IsActive, request.SortOrder),
            cancellationToken);

        return Success(response, "Service updated successfully.");
    }

    [HttpDelete("{serviceId:long}")]
    [Authorize(Policy = PermissionNames.LookupManage)]
    public async Task<ActionResult<ApiResponse<bool>>> DeleteServiceAsync(
        [FromRoute] long serviceId,
        CancellationToken cancellationToken)
    {
        await _sender.Send(new DeleteServiceCommand(serviceId), cancellationToken);
        return Success(true, "Service deleted successfully.");
    }

    // ── Service image (existing) ──────────────────────────────────────────────────────────────────
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

    // ── Service AI image prompt ────────────────────────────────────────────────────────────────────
    [HttpPut("{serviceId:long}/image-prompt")]
    [Authorize(Policy = PermissionNames.LookupManage)]
    public async Task<ActionResult<ApiResponse<ServiceLookupResponse>>> SetPromptAsync(
        [FromRoute] long serviceId,
        [FromBody] SetServicePromptRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _sender.Send(
            new SetServicePromptCommand(serviceId, request.ImageAIPrompt),
            cancellationToken);

        return Success(response, "Service image prompt updated successfully.");
    }
}
