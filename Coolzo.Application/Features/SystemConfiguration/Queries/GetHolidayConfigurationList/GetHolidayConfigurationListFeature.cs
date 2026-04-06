using Coolzo.Application.Common.Interfaces;
using Coolzo.Application.Common.Mappings;
using Coolzo.Contracts.Responses.Admin;
using FluentValidation;
using MediatR;

namespace Coolzo.Application.Features.SystemConfiguration.Queries.GetHolidayConfigurationList;

public sealed record GetHolidayConfigurationListQuery(
    int? Year,
    bool? IsActive) : IRequest<IReadOnlyCollection<HolidayConfigurationResponse>>;

public sealed class GetHolidayConfigurationListQueryValidator : AbstractValidator<GetHolidayConfigurationListQuery>
{
    public GetHolidayConfigurationListQueryValidator()
    {
        RuleFor(request => request.Year).InclusiveBetween(2000, 2100).When(request => request.Year.HasValue);
    }
}

public sealed class GetHolidayConfigurationListQueryHandler : IRequestHandler<GetHolidayConfigurationListQuery, IReadOnlyCollection<HolidayConfigurationResponse>>
{
    private readonly IAdminConfigurationRepository _adminConfigurationRepository;

    public GetHolidayConfigurationListQueryHandler(IAdminConfigurationRepository adminConfigurationRepository)
    {
        _adminConfigurationRepository = adminConfigurationRepository;
    }

    public async Task<IReadOnlyCollection<HolidayConfigurationResponse>> Handle(GetHolidayConfigurationListQuery request, CancellationToken cancellationToken)
    {
        var entities = await _adminConfigurationRepository.SearchHolidaysAsync(request.Year, request.IsActive, cancellationToken);

        return entities.Select(AdminResponseMapper.ToResponse).ToArray();
    }
}
