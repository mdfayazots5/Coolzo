using Coolzo.Application.Common.Interfaces;
using Coolzo.Application.Common.Mappings;
using Coolzo.Contracts.Responses.Admin;
using FluentValidation;
using MediatR;

namespace Coolzo.Application.Features.CMS.Queries.GetCMSBlockList;

public sealed record GetCMSBlockListQuery(
    string? Search,
    bool? IsActive,
    bool? IsPublished) : IRequest<IReadOnlyCollection<CMSBlockResponse>>;

public sealed class GetCMSBlockListQueryValidator : AbstractValidator<GetCMSBlockListQuery>
{
    public GetCMSBlockListQueryValidator()
    {
        RuleFor(request => request.Search).MaximumLength(128);
    }
}

public sealed class GetCMSBlockListQueryHandler : IRequestHandler<GetCMSBlockListQuery, IReadOnlyCollection<CMSBlockResponse>>
{
    private readonly IAdminConfigurationRepository _adminConfigurationRepository;

    public GetCMSBlockListQueryHandler(IAdminConfigurationRepository adminConfigurationRepository)
    {
        _adminConfigurationRepository = adminConfigurationRepository;
    }

    public async Task<IReadOnlyCollection<CMSBlockResponse>> Handle(GetCMSBlockListQuery request, CancellationToken cancellationToken)
    {
        var entities = await _adminConfigurationRepository.SearchCmsBlocksAsync(request.Search, request.IsActive, request.IsPublished, cancellationToken);

        return entities.Select(entity => AdminResponseMapper.ToResponse(entity)).ToArray();
    }
}
