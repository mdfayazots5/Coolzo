using Coolzo.Application.Common.Interfaces;
using Coolzo.Application.Common.Mappings;
using Coolzo.Contracts.Responses.Admin;
using Coolzo.Shared.Constants;
using Coolzo.Shared.Exceptions;
using FluentValidation;
using MediatR;

namespace Coolzo.Application.Features.NotificationTemplate.Queries.GetNotificationTemplateDetail;

public sealed record GetNotificationTemplateDetailQuery(long NotificationTemplateId) : IRequest<NotificationTemplateResponse>;

public sealed class GetNotificationTemplateDetailQueryValidator : AbstractValidator<GetNotificationTemplateDetailQuery>
{
    public GetNotificationTemplateDetailQueryValidator()
    {
        RuleFor(request => request.NotificationTemplateId).GreaterThan(0);
    }
}

public sealed class GetNotificationTemplateDetailQueryHandler : IRequestHandler<GetNotificationTemplateDetailQuery, NotificationTemplateResponse>
{
    private readonly IAdminConfigurationRepository _adminConfigurationRepository;

    public GetNotificationTemplateDetailQueryHandler(IAdminConfigurationRepository adminConfigurationRepository)
    {
        _adminConfigurationRepository = adminConfigurationRepository;
    }

    public async Task<NotificationTemplateResponse> Handle(GetNotificationTemplateDetailQuery request, CancellationToken cancellationToken)
    {
        var entity = await _adminConfigurationRepository.GetNotificationTemplateByIdAsync(request.NotificationTemplateId, cancellationToken)
            ?? throw new AppException(ErrorCodes.NotFound, "The requested notification template could not be found.", 404);

        return AdminResponseMapper.ToResponse(entity);
    }
}
