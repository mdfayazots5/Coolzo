using Coolzo.Application.Common.Interfaces;
using Coolzo.Application.Common.Mappings;
using Coolzo.Contracts.Responses.Admin;
using Coolzo.Shared.Constants;
using Coolzo.Shared.Exceptions;
using FluentValidation;
using MediatR;

namespace Coolzo.Application.Features.CMS.Queries.GetPublicServiceContent;

public sealed record GetPublicServiceContentQuery(string BlockKey) : IRequest<CMSBlockResponse>;

public sealed class GetPublicServiceContentQueryValidator : AbstractValidator<GetPublicServiceContentQuery>
{
    public GetPublicServiceContentQueryValidator()
    {
        RuleFor(request => request.BlockKey).NotEmpty().MaximumLength(128);
    }
}

public sealed class GetPublicServiceContentQueryHandler : IRequestHandler<GetPublicServiceContentQuery, CMSBlockResponse>
{
    private readonly IAdminConfigurationRepository _adminConfigurationRepository;

    public GetPublicServiceContentQueryHandler(IAdminConfigurationRepository adminConfigurationRepository)
    {
        _adminConfigurationRepository = adminConfigurationRepository;
    }

    public async Task<CMSBlockResponse> Handle(GetPublicServiceContentQuery request, CancellationToken cancellationToken)
    {
        var block = await _adminConfigurationRepository.GetCmsBlockByKeyAsync(request.BlockKey.Trim(), publishedOnly: true, cancellationToken)
            ?? throw new AppException(ErrorCodes.NotFound, "The requested service content could not be found.", 404);

        return AdminResponseMapper.ToResponse(block);
    }
}
