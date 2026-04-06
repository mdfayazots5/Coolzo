using Coolzo.Application.Common.Interfaces;
using Coolzo.Application.Common.Mappings;
using Coolzo.Contracts.Responses.Admin;
using Coolzo.Domain.Entities;
using Coolzo.Shared.Constants;
using Coolzo.Shared.Exceptions;
using FluentValidation;
using MediatR;

namespace Coolzo.Application.Features.CommunicationPreference.Queries.GetCommunicationPreferenceByCustomer;

public sealed record GetCommunicationPreferenceByCustomerQuery(long CustomerId) : IRequest<CommunicationPreferenceResponse>;

public sealed class GetCommunicationPreferenceByCustomerQueryValidator : AbstractValidator<GetCommunicationPreferenceByCustomerQuery>
{
    public GetCommunicationPreferenceByCustomerQueryValidator()
    {
        RuleFor(request => request.CustomerId).GreaterThan(0);
    }
}

public sealed class GetCommunicationPreferenceByCustomerQueryHandler : IRequestHandler<GetCommunicationPreferenceByCustomerQuery, CommunicationPreferenceResponse>
{
    private readonly IAdminConfigurationRepository _adminConfigurationRepository;
    private readonly IBookingRepository _bookingRepository;

    public GetCommunicationPreferenceByCustomerQueryHandler(
        IAdminConfigurationRepository adminConfigurationRepository,
        IBookingRepository bookingRepository)
    {
        _adminConfigurationRepository = adminConfigurationRepository;
        _bookingRepository = bookingRepository;
    }

    public async Task<CommunicationPreferenceResponse> Handle(GetCommunicationPreferenceByCustomerQuery request, CancellationToken cancellationToken)
    {
        var customer = await _bookingRepository.GetCustomerByIdAsync(request.CustomerId, cancellationToken)
            ?? throw new AppException(ErrorCodes.NotFound, "The requested customer could not be found.", 404);

        var entity = await _adminConfigurationRepository.GetCommunicationPreferenceByCustomerIdAsync(customer.CustomerId, cancellationToken);

        return entity is not null
            ? AdminResponseMapper.ToResponse(entity)
            : CreateDefaultResponse(customer);
    }

    private static CommunicationPreferenceResponse CreateDefaultResponse(Customer customer)
    {
        return new CommunicationPreferenceResponse(
            0,
            customer.CustomerId,
            customer.EmailAddress,
            customer.MobileNumber,
            true,
            !string.IsNullOrWhiteSpace(customer.MobileNumber),
            false,
            false,
            false,
            null);
    }
}
