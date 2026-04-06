using Coolzo.Application.Common.Interfaces;
using Coolzo.Application.Common.Mappings;
using Coolzo.Contracts.Responses.Admin;
using FluentValidation;
using MediatR;

namespace Coolzo.Application.Features.MasterDataAdmin.Queries.GetDynamicMasterRecordList;

public sealed record GetDynamicMasterRecordListQuery(
    string? MasterType,
    string? Search,
    bool? IsActive) : IRequest<IReadOnlyCollection<DynamicMasterRecordResponse>>;

public sealed class GetDynamicMasterRecordListQueryValidator : AbstractValidator<GetDynamicMasterRecordListQuery>
{
    public GetDynamicMasterRecordListQueryValidator()
    {
        RuleFor(request => request.MasterType).MaximumLength(128);
        RuleFor(request => request.Search).MaximumLength(128);
    }
}

public sealed class GetDynamicMasterRecordListQueryHandler : IRequestHandler<GetDynamicMasterRecordListQuery, IReadOnlyCollection<DynamicMasterRecordResponse>>
{
    private readonly IAdminConfigurationRepository _adminConfigurationRepository;

    public GetDynamicMasterRecordListQueryHandler(IAdminConfigurationRepository adminConfigurationRepository)
    {
        _adminConfigurationRepository = adminConfigurationRepository;
    }

    public async Task<IReadOnlyCollection<DynamicMasterRecordResponse>> Handle(GetDynamicMasterRecordListQuery request, CancellationToken cancellationToken)
    {
        var entities = await _adminConfigurationRepository.SearchDynamicMasterRecordsAsync(
            request.MasterType,
            request.Search,
            request.IsActive,
            cancellationToken);

        return entities.Select(AdminResponseMapper.ToResponse).ToArray();
    }
}
