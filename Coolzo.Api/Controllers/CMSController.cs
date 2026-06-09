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
using Coolzo.Application.Features.CMS.Snapshot.Commands.PublishContentSnapshot;
using Coolzo.Application.Features.CMS.Snapshot.Commands.RollbackContentSnapshot;
using Coolzo.Application.Features.CMS.Snapshot.Queries.GetContentSnapshot;
using Coolzo.Application.Features.CMS.Snapshot.Queries.GetSnapshotManifest;
using Coolzo.Application.Features.CMS.ScreenImage.Commands.UploadScreenImage;
using Coolzo.Application.Features.CMS.ScreenImage.Commands.UpsertScreenImageSlot;
using Coolzo.Application.Features.CMS.ScreenImage.Queries.GetScreenImageSlotList;
using Coolzo.Application.Features.CMS.Theme.Commands.UpdateTheme;
using Coolzo.Application.Features.CMS.Theme.Queries.GetTheme;
using Coolzo.Contracts.Common;
using Coolzo.Contracts.Requests.CMS;
using Coolzo.Contracts.Responses.CMS;
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

    [Authorize(Policy = PermissionNames.CmsManage)]
    [HttpPost("publish")]
    public async Task<ActionResult<ApiResponse<SnapshotManifestResponse>>> PublishSnapshotAsync(
        CancellationToken cancellationToken)
    {
        var response = await _sender.Send(new PublishContentSnapshotCommand(), cancellationToken);

        return Success(response, "Content snapshot published successfully.");
    }

    [Authorize(Policy = PermissionNames.CmsManage)]
    [HttpPost("rollback/{version:int}")]
    public async Task<ActionResult<ApiResponse<SnapshotManifestResponse>>> RollbackSnapshotAsync(
        [FromRoute] int version,
        CancellationToken cancellationToken)
    {
        var response = await _sender.Send(new RollbackContentSnapshotCommand(version), cancellationToken);

        return Success(response, "Content snapshot rolled back successfully.");
    }

    [AllowAnonymous]
    [HttpGet("snapshot/manifest")]
    public async Task<ActionResult<ApiResponse<SnapshotManifestResponse>>> GetSnapshotManifestAsync(
        CancellationToken cancellationToken)
    {
        var response = await _sender.Send(new GetSnapshotManifestQuery(), cancellationToken);

        return Success(response);
    }

    [AllowAnonymous]
    [HttpGet("snapshot/{version:int}")]
    public async Task<ActionResult<ApiResponse<ContentSnapshotResponse>>> GetSnapshotByVersionAsync(
        [FromRoute] int version,
        CancellationToken cancellationToken)
    {
        var response = await _sender.Send(new GetContentSnapshotQuery(version), cancellationToken);

        return Success(response);
    }

    [Authorize(Policy = PermissionNames.CmsRead)]
    [HttpGet("admin/theme")]
    public async Task<ActionResult<ApiResponse<ThemeResponse>>> GetThemeAsync(CancellationToken cancellationToken)
    {
        var response = await _sender.Send(new GetThemeQuery(), cancellationToken);

        return Success(response);
    }

    [Authorize(Policy = PermissionNames.CmsManage)]
    [HttpPut("admin/theme")]
    public async Task<ActionResult<ApiResponse<ThemeResponse>>> UpdateThemeAsync(
        [FromBody] UpdateThemeRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _sender.Send(new UpdateThemeCommand(request.Tokens), cancellationToken);

        return Success(response, "Theme updated successfully.");
    }

    [Authorize(Policy = PermissionNames.CmsRead)]
    [HttpGet("admin/image-slots")]
    public async Task<ActionResult<ApiResponse<IReadOnlyCollection<ScreenImageSlotResponse>>>> GetImageSlotsAsync(
        [FromQuery] string? pageKey,
        [FromQuery] bool? isActive,
        CancellationToken cancellationToken)
    {
        var response = await _sender.Send(new GetScreenImageSlotListQuery(pageKey, isActive), cancellationToken);

        return Success(response);
    }

    [Authorize(Policy = PermissionNames.CmsManage)]
    [HttpPost("admin/image-slots")]
    public async Task<ActionResult<ApiResponse<ScreenImageSlotResponse>>> CreateImageSlotAsync(
        [FromBody] ScreenImageSlotUpsertRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _sender.Send(
            new UpsertScreenImageSlotCommand(
                null,
                request.PageKey,
                request.SlotKey,
                request.Breakpoint,
                request.RecommendedWidth,
                request.RecommendedHeight,
                request.AltText,
                request.SuggestedAIPrompt,
                request.IsActive),
            cancellationToken);

        return Success(response, "Screen image slot created successfully.");
    }

    [Authorize(Policy = PermissionNames.CmsManage)]
    [HttpPut("admin/image-slots/{screenImageSlotId:long}")]
    public async Task<ActionResult<ApiResponse<ScreenImageSlotResponse>>> UpdateImageSlotAsync(
        [FromRoute] long screenImageSlotId,
        [FromBody] ScreenImageSlotUpsertRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _sender.Send(
            new UpsertScreenImageSlotCommand(
                screenImageSlotId,
                request.PageKey,
                request.SlotKey,
                request.Breakpoint,
                request.RecommendedWidth,
                request.RecommendedHeight,
                request.AltText,
                request.SuggestedAIPrompt,
                request.IsActive),
            cancellationToken);

        return Success(response, "Screen image slot updated successfully.");
    }

    [Authorize(Policy = PermissionNames.CmsManage)]
    [HttpPost("admin/image-slots/{screenImageSlotId:long}/upload")]
    public async Task<ActionResult<ApiResponse<ScreenImageSlotResponse>>> UploadImageAsync(
        [FromRoute] long screenImageSlotId,
        [FromBody] ScreenImageUploadRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _sender.Send(
            new UploadScreenImageCommand(
                screenImageSlotId,
                request.FileName,
                request.ContentType,
                request.Base64Content,
                request.AltText),
            cancellationToken);

        return Success(response, "Screen image uploaded successfully.");
    }
}
