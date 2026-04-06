using Coolzo.Application.Common.Interfaces;
using Coolzo.Application.Common.Mappings;
using Coolzo.Contracts.Responses.Admin;
using FluentValidation;
using MediatR;

namespace Coolzo.Application.Features.NotificationTemplate.Queries.GetNotificationTemplateList;

public sealed record GetNotificationTemplateListQuery(
    string? Search,
    string? Channel,
    string? TriggerCode,
    bool? IsActive) : IRequest<IReadOnlyCollection<NotificationTemplateResponse>>;

public sealed class GetNotificationTemplateListQueryValidator : AbstractValidator<GetNotificationTemplateListQuery>
{
    public GetNotificationTemplateListQueryValidator()
    {
        RuleFor(request => request.Search).MaximumLength(128);
        RuleFor(request => request.Channel).MaximumLength(32);
        RuleFor(request => request.TriggerCode).MaximumLength(128);
    }
}

public sealed class GetNotificationTemplateListQueryHandler : IRequestHandler<GetNotificationTemplateListQuery, IReadOnlyCollection<NotificationTemplateResponse>>
{
    private readonly IAdminConfigurationRepository _adminConfigurationRepository;

    public GetNotificationTemplateListQueryHandler(IAdminConfigurationRepository adminConfigurationRepository)
    {
        _adminConfigurationRepository = adminConfigurationRepository;
    }

    public async Task<IReadOnlyCollection<NotificationTemplateResponse>> Handle(GetNotificationTemplateListQuery request, CancellationToken cancellationToken)
    {
        var entities = await _adminConfigurationRepository.SearchNotificationTemplatesAsync(
            request.Search,
            request.Channel,
            request.TriggerCode,
            request.IsActive,
            cancellationToken);

        return entities.Select(AdminResponseMapper.ToResponse).ToArray();
    }
}
