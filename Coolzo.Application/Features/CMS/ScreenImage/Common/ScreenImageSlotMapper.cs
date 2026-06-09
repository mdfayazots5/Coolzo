using Coolzo.Contracts.Responses.CMS;
using Coolzo.Domain.Entities;

namespace Coolzo.Application.Features.CMS.ScreenImage.Common;

internal static class ScreenImageSlotMapper
{
    public static ScreenImageSlotResponse ToResponse(ScreenImageSlot entity)
    {
        return new ScreenImageSlotResponse(
            entity.ScreenImageSlotId,
            entity.PageKey,
            entity.SlotKey,
            entity.Breakpoint,
            entity.RecommendedWidth,
            entity.RecommendedHeight,
            entity.AltText,
            entity.SuggestedAIPrompt,
            entity.ImageUrl,
            entity.IsActive);
    }
}
