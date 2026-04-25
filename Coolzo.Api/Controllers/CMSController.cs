using Coolzo.Application.Features.CMS.Commands.CreateCMSBanner;
using Coolzo.Application.Features.CMS.Commands.CreateCMSBlock;
using Coolzo.Application.Features.CMS.Commands.CreateCMSFaq;
using Coolzo.Application.Features.CMS.Commands.UpdateCMSBanner;
using Coolzo.Application.Features.CMS.Commands.UpdateCMSBlock;
using Coolzo.Application.Features.CMS.Commands.UpdateCMSFaq;
using Coolzo.Application.Features.CMS.Queries.GetCMSBannerList;
using Coolzo.Application.Features.CMS.Queries.GetCMSBlockList;
using Coolzo.Application.Features.CMS.Queries.GetCMSFaqList;
using Coolzo.Application.Features.CMS.Queries.GetPublicBannerContent;
using Coolzo.Application.Features.CMS.Queries.GetPublicFAQContent;
using Coolzo.Application.Features.CMS.Queries.GetPublicHomeCMSContent;
using Coolzo.Application.Features.CMS.Queries.GetPublicServiceContent;
using Coolzo.Contracts.Common;
using Coolzo.Contracts.Requests.Admin;
using Coolzo.Contracts.Responses.Admin;
using Coolzo.Shared.Constants;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Coolzo.Api.Controllers;

[Route("api/cms")]
public sealed class CMSController : ApiControllerBase
{
    private readonly ISender _sender;

    public CMSController(ISender sender)
    {
        _sender = sender;
    }

    [AllowAnonymous]
    [HttpGet("public/home")]
    public async Task<ActionResult<ApiResponse<PublicHomeCMSContentResponse>>> GetPublicHomeAsync(CancellationToken cancellationToken)
    {
        var response = await _sender.Send(new GetPublicHomeCMSContentQuery(), cancellationToken);

        return Success(response);
    }

    [AllowAnonymous]
    [HttpGet("public/faqs")]
    public async Task<ActionResult<ApiResponse<IReadOnlyCollection<CMSFaqResponse>>>> GetPublicFaqsAsync(CancellationToken cancellationToken)
    {
        var response = await _sender.Send(new GetPublicFAQContentQuery(), cancellationToken);

        return Success(response);
    }

    [AllowAnonymous]
    [HttpGet("public/banners")]
    public async Task<ActionResult<ApiResponse<IReadOnlyCollection<CMSBannerResponse>>>> GetPublicBannersAsync(CancellationToken cancellationToken)
    {
        var response = await _sender.Send(new GetPublicBannerContentQuery(), cancellationToken);

        return Success(response);
    }

    [AllowAnonymous]
    [HttpGet("public/service-content/{key}")]
    public async Task<ActionResult<ApiResponse<CMSBlockResponse>>> GetPublicServiceContentAsync(
        [FromRoute] string key,
        CancellationToken cancellationToken)
    {
        var response = await _sender.Send(new GetPublicServiceContentQuery(key), cancellationToken);

        return Success(response);
    }

    [AllowAnonymous]
    [HttpGet("blocks/{key}")]
    public async Task<ActionResult<ApiResponse<CMSBlockResponse>>> GetPublicBlockByKeyAsync(
        [FromRoute] string key,
        CancellationToken cancellationToken)
    {
        var response = await _sender.Send(new GetPublicServiceContentQuery(key), cancellationToken);

        return Success(response);
    }

    [Authorize(Policy = PermissionNames.CmsRead)]
    [HttpGet("admin/blocks")]
    public async Task<ActionResult<ApiResponse<IReadOnlyCollection<CMSBlockResponse>>>> GetBlocksAsync(
        [FromQuery] string? search,
        [FromQuery] bool? isActive,
        [FromQuery] bool? isPublished,
        CancellationToken cancellationToken)
    {
        var response = await _sender.Send(new GetCMSBlockListQuery(search, isActive, isPublished), cancellationToken);

        return Success(response);
    }

    [Authorize(Policy = PermissionNames.CmsManage)]
    [HttpPost("admin/blocks")]
    public async Task<ActionResult<ApiResponse<CMSBlockResponse>>> CreateBlockAsync(
        [FromBody] CMSBlockUpsertRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _sender.Send(
            new CreateCMSBlockCommand(
                request.BlockKey,
                request.Title,
                request.Summary,
                request.Content,
                request.PreviewImageUrl,
                request.IsActive,
                request.IsPublished,
                request.SortOrder),
            cancellationToken);

        return Success(response, "CMS block created successfully.");
    }

    [Authorize(Policy = PermissionNames.CmsManage)]
    [HttpPut("admin/blocks/{cmsBlockId:long}")]
    public async Task<ActionResult<ApiResponse<CMSBlockResponse>>> UpdateBlockAsync(
        [FromRoute] long cmsBlockId,
        [FromBody] CMSBlockUpsertRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _sender.Send(
            new UpdateCMSBlockCommand(
                cmsBlockId,
                request.BlockKey,
                request.Title,
                request.Summary,
                request.Content,
                request.PreviewImageUrl,
                request.IsActive,
                request.IsPublished,
                request.SortOrder),
            cancellationToken);

        return Success(response, "CMS block updated successfully.");
    }

    [Authorize(Policy = PermissionNames.CmsRead)]
    [HttpGet("admin/banners")]
    public async Task<ActionResult<ApiResponse<IReadOnlyCollection<CMSBannerResponse>>>> GetBannersAsync(
        [FromQuery] string? search,
        [FromQuery] bool? isActive,
        [FromQuery] bool? isPublished,
        CancellationToken cancellationToken)
    {
        var response = await _sender.Send(new GetCMSBannerListQuery(search, isActive, isPublished), cancellationToken);

        return Success(response);
    }

    [Authorize(Policy = PermissionNames.CmsManage)]
    [HttpPost("admin/banners")]
    public async Task<ActionResult<ApiResponse<CMSBannerResponse>>> CreateBannerAsync(
        [FromBody] CMSBannerUpsertRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _sender.Send(
            new CreateCMSBannerCommand(
                request.BannerTitle,
                request.BannerSubtitle,
                request.ImageUrl,
                request.RedirectUrl,
                request.DisplayArea,
                request.ActiveFromDate,
                request.ActiveToDate,
                request.IsActive,
                request.IsPublished,
                request.SortOrder),
            cancellationToken);

        return Success(response, "CMS banner created successfully.");
    }

    [Authorize(Policy = PermissionNames.CmsManage)]
    [HttpPut("admin/banners/{cmsBannerId:long}")]
    public async Task<ActionResult<ApiResponse<CMSBannerResponse>>> UpdateBannerAsync(
        [FromRoute] long cmsBannerId,
        [FromBody] CMSBannerUpsertRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _sender.Send(
            new UpdateCMSBannerCommand(
                cmsBannerId,
                request.BannerTitle,
                request.BannerSubtitle,
                request.ImageUrl,
                request.RedirectUrl,
                request.DisplayArea,
                request.ActiveFromDate,
                request.ActiveToDate,
                request.IsActive,
                request.IsPublished,
                request.SortOrder),
            cancellationToken);

        return Success(response, "CMS banner updated successfully.");
    }

    [Authorize(Policy = PermissionNames.CmsRead)]
    [HttpGet("admin/faqs")]
    public async Task<ActionResult<ApiResponse<IReadOnlyCollection<CMSFaqResponse>>>> GetFaqsAsync(
        [FromQuery] string? category,
        [FromQuery] string? search,
        [FromQuery] bool? isActive,
        [FromQuery] bool? isPublished,
        CancellationToken cancellationToken)
    {
        var response = await _sender.Send(new GetCMSFaqListQuery(category, search, isActive, isPublished), cancellationToken);

        return Success(response);
    }

    [Authorize(Policy = PermissionNames.CmsManage)]
    [HttpPost("admin/faqs")]
    public async Task<ActionResult<ApiResponse<CMSFaqResponse>>> CreateFaqAsync(
        [FromBody] CMSFaqUpsertRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _sender.Send(
            new CreateCMSFaqCommand(
                request.Category,
                request.Question,
                request.Answer,
                request.IsActive,
                request.IsPublished,
                request.SortOrder),
            cancellationToken);

        return Success(response, "CMS FAQ created successfully.");
    }

    [Authorize(Policy = PermissionNames.CmsManage)]
    [HttpPut("admin/faqs/{cmsFaqId:long}")]
    public async Task<ActionResult<ApiResponse<CMSFaqResponse>>> UpdateFaqAsync(
        [FromRoute] long cmsFaqId,
        [FromBody] CMSFaqUpsertRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _sender.Send(
            new UpdateCMSFaqCommand(
                cmsFaqId,
                request.Category,
                request.Question,
                request.Answer,
                request.IsActive,
                request.IsPublished,
                request.SortOrder),
            cancellationToken);

        return Success(response, "CMS FAQ updated successfully.");
    }
}
