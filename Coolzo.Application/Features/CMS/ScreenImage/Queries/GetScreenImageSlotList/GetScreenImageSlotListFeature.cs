using Coolzo.Application.Common.Interfaces;
using Coolzo.Application.Features.CMS.ScreenImage.Common;
using Coolzo.Contracts.Responses.CMS;
using MediatR;

namespace Coolzo.Application.Features.CMS.ScreenImage.Queries.GetScreenImageSlotList;

public sealed record GetScreenImageSlotListQuery(string? PageKey, bool? IsActive)
    : IRequest<IReadOnlyCollection<ScreenImageSlotResponse>>;

public sealed class GetScreenImageSlotListQueryHandler
    : IRequestHandler<GetScreenImageSlotListQuery, IReadOnlyCollection<ScreenImageSlotResponse>>
{
    private readonly IScreenImageSlotRepository _screenImageSlotRepository;

    public GetScreenImageSlotListQueryHandler(IScreenImageSlotRepository screenImageSlotRepository)
    {
        _screenImageSlotRepository = screenImageSlotRepository;
    }

    public async Task<IReadOnlyCollection<ScreenImageSlotResponse>> Handle(
        GetScreenImageSlotListQuery request,
        CancellationToken cancellationToken)
    {
        var slots = await _screenImageSlotRepository.SearchAsync(request.PageKey, request.IsActive, cancellationToken);

        return slots.Select(ScreenImageSlotMapper.ToResponse).ToArray();
    }
}
