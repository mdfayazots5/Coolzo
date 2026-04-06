using Coolzo.Application.Common.Interfaces;
using Coolzo.Application.Common.Mappings;
using Coolzo.Contracts.Responses.Admin;
using FluentValidation;
using MediatR;

namespace Coolzo.Application.Features.NotificationTriggerConfiguration.Queries.GetNotificationTriggerList;

public sealed record GetNotificationTriggerListQuery(
    string? Search,
    bool? IsEnabled) : IRequest<IReadOnlyCollection<NotificationTriggerConfigurationResponse>>;

public sealed class GetNotificationTriggerListQueryValidator : AbstractValidator<GetNotificationTriggerListQuery>
{
    public GetNotificationTriggerListQueryValidator()
    {
        RuleFor(request => request.Search).MaximumLength(128);
    }
}

public sealed class GetNotificationTriggerListQueryHandler : IRequestHandler<GetNotificationTriggerListQuery, IReadOnlyCollection<NotificationTriggerConfigurationResponse>>
{
    private readonly IAdminConfigurationRepository _adminConfigurationRepository;

    public GetNotificationTriggerListQueryHandler(IAdminConfigurationRepository adminConfigurationRepository)
    {
        _adminConfigurationRepository = adminConfigurationRepository;
    }

    public async Task<IReadOnlyCollection<NotificationTriggerConfigurationResponse>> Handle(GetNotificationTriggerListQuery request, CancellationToken cancellationToken)
    {
        var entities = await _adminConfigurationRepository.SearchNotificationTriggersAsync(request.Search, request.IsEnabled, cancellationToken);

        return entities.Select(AdminResponseMapper.ToResponse).ToArray();
    }
}
