using Coolzo.Application.Common.Interfaces;
using Coolzo.Application.Common.Mappings;
using Coolzo.Contracts.Responses.Admin;
using Coolzo.Shared.Constants;
using Coolzo.Shared.Exceptions;
using FluentValidation;
using MediatR;

namespace Coolzo.Application.Features.SystemConfiguration.Queries.GetSystemConfigurationDetail;

public sealed record GetSystemConfigurationDetailQuery(long SystemConfigurationId) : IRequest<SystemConfigurationResponse>;

public sealed class GetSystemConfigurationDetailQueryValidator : AbstractValidator<GetSystemConfigurationDetailQuery>
{
    public GetSystemConfigurationDetailQueryValidator()
    {
        RuleFor(request => request.SystemConfigurationId).GreaterThan(0);
    }
}

public sealed class GetSystemConfigurationDetailQueryHandler : IRequestHandler<GetSystemConfigurationDetailQuery, SystemConfigurationResponse>
{
    private readonly IAdminConfigurationRepository _adminConfigurationRepository;

    public GetSystemConfigurationDetailQueryHandler(IAdminConfigurationRepository adminConfigurationRepository)
    {
        _adminConfigurationRepository = adminConfigurationRepository;
    }

    public async Task<SystemConfigurationResponse> Handle(GetSystemConfigurationDetailQuery request, CancellationToken cancellationToken)
    {
        var entity = await _adminConfigurationRepository.GetSystemConfigurationByIdAsync(request.SystemConfigurationId, cancellationToken)
            ?? throw new AppException(ErrorCodes.NotFound, "The requested system configuration could not be found.", 404);

        return AdminResponseMapper.ToResponse(entity, maskSensitive: false);
    }
}
