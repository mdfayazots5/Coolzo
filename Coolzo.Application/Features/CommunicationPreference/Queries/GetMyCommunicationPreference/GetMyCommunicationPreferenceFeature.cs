using Coolzo.Application.Common.Interfaces;
using Coolzo.Application.Common.Mappings;
using Coolzo.Contracts.Responses.Admin;
using Coolzo.Shared.Constants;
using Coolzo.Shared.Exceptions;
using FluentValidation;
using MediatR;

namespace Coolzo.Application.Features.CommunicationPreference.Queries.GetMyCommunicationPreference;

public sealed record GetMyCommunicationPreferenceQuery : IRequest<CommunicationPreferenceResponse>;

public sealed class GetMyCommunicationPreferenceQueryHandler : IRequestHandler<GetMyCommunicationPreferenceQuery, CommunicationPreferenceResponse>
{
    private readonly IAdminConfigurationRepository _adminConfigurationRepository;
    private readonly IBookingRepository _bookingRepository;
    private readonly ICurrentUserContext _currentUserContext;
    private readonly IUserRepository _userRepository;

    public GetMyCommunicationPreferenceQueryHandler(
        IAdminConfigurationRepository adminConfigurationRepository,
        IBookingRepository bookingRepository,
        IUserRepository userRepository,
        ICurrentUserContext currentUserContext)
    {
        _adminConfigurationRepository = adminConfigurationRepository;
        _bookingRepository = bookingRepository;
        _userRepository = userRepository;
        _currentUserContext = currentUserContext;
    }

    public async Task<CommunicationPreferenceResponse> Handle(GetMyCommunicationPreferenceQuery request, CancellationToken cancellationToken)
    {
        if (!_currentUserContext.IsAuthenticated || !_currentUserContext.UserId.HasValue)
        {
            throw new AppException(ErrorCodes.Unauthorized, "Customer authentication is required.", 401);
        }

        var customer = await _bookingRepository.GetCustomerByUserIdAsync(_currentUserContext.UserId.Value, cancellationToken);

        if (customer is not null)
        {
            var entity = await _adminConfigurationRepository.GetCommunicationPreferenceByCustomerIdAsync(customer.CustomerId, cancellationToken);

            return entity is not null
                ? AdminResponseMapper.ToResponse(entity)
                : new CommunicationPreferenceResponse(
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

        var user = await _userRepository.GetByIdWithRolesAsync(_currentUserContext.UserId.Value, cancellationToken)
            ?? throw new AppException(ErrorCodes.NotFound, "The authenticated user could not be resolved.", 404);

        return new CommunicationPreferenceResponse(
            0,
            0,
            user.Email,
            string.Empty,
            true,
            false,
            false,
            false,
            false,
            null);
    }
}
