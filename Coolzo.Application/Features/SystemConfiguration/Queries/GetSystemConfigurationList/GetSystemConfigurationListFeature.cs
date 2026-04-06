using Coolzo.Application.Common.Interfaces;
using Coolzo.Application.Common.Mappings;
using Coolzo.Contracts.Responses.Admin;
using FluentValidation;
using MediatR;

namespace Coolzo.Application.Features.SystemConfiguration.Queries.GetSystemConfigurationList;

public sealed record GetSystemConfigurationListQuery(
    string? ConfigurationGroup,
    string? ConfigurationKey,
    string? ValueType,
    bool? IsActive) : IRequest<IReadOnlyCollection<SystemConfigurationResponse>>;

public sealed class GetSystemConfigurationListQueryValidator : AbstractValidator<GetSystemConfigurationListQuery>
{
    public GetSystemConfigurationListQueryValidator()
    {
        RuleFor(request => request.ConfigurationGroup).MaximumLength(128);
        RuleFor(request => request.ConfigurationKey).MaximumLength(128);
        RuleFor(request => request.ValueType).MaximumLength(64);
    }
}

public sealed class GetSystemConfigurationListQueryHandler : IRequestHandler<GetSystemConfigurationListQuery, IReadOnlyCollection<SystemConfigurationResponse>>
{
    private readonly IAdminConfigurationRepository _adminConfigurationRepository;

    public GetSystemConfigurationListQueryHandler(IAdminConfigurationRepository adminConfigurationRepository)
    {
        _adminConfigurationRepository = adminConfigurationRepository;
    }

    public async Task<IReadOnlyCollection<SystemConfigurationResponse>> Handle(GetSystemConfigurationListQuery request, CancellationToken cancellationToken)
    {
        var entities = await _adminConfigurationRepository.SearchSystemConfigurationsAsync(
            request.ConfigurationGroup,
            request.ConfigurationKey,
            request.ValueType,
            request.IsActive,
            cancellationToken);

        return entities
            .Select(entity => AdminResponseMapper.ToResponse(entity))
            .ToArray();
    }
}
