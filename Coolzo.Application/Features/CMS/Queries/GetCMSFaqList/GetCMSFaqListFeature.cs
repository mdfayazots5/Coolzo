using Coolzo.Application.Common.Interfaces;
using Coolzo.Application.Common.Mappings;
using Coolzo.Contracts.Responses.Admin;
using FluentValidation;
using MediatR;

namespace Coolzo.Application.Features.CMS.Queries.GetCMSFaqList;

public sealed record GetCMSFaqListQuery(
    string? Category,
    string? Search,
    bool? IsActive,
    bool? IsPublished) : IRequest<IReadOnlyCollection<CMSFaqResponse>>;

public sealed class GetCMSFaqListQueryValidator : AbstractValidator<GetCMSFaqListQuery>
{
    public GetCMSFaqListQueryValidator()
    {
        RuleFor(request => request.Category).MaximumLength(128);
        RuleFor(request => request.Search).MaximumLength(128);
    }
}

public sealed class GetCMSFaqListQueryHandler : IRequestHandler<GetCMSFaqListQuery, IReadOnlyCollection<CMSFaqResponse>>
{
    private readonly IAdminConfigurationRepository _adminConfigurationRepository;

    public GetCMSFaqListQueryHandler(IAdminConfigurationRepository adminConfigurationRepository)
    {
        _adminConfigurationRepository = adminConfigurationRepository;
    }

    public async Task<IReadOnlyCollection<CMSFaqResponse>> Handle(GetCMSFaqListQuery request, CancellationToken cancellationToken)
    {
        var entities = await _adminConfigurationRepository.SearchCmsFaqsAsync(
            request.Category,
            request.Search,
            request.IsActive,
            request.IsPublished,
            publicOnly: false,
            cancellationToken);

        return entities.Select(AdminResponseMapper.ToResponse).ToArray();
    }
}
