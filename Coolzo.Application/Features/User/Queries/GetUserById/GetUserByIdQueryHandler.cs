using Coolzo.Application.Common.Interfaces;
using Coolzo.Contracts.Responses.User;
using Coolzo.Shared.Constants;
using Coolzo.Shared.Exceptions;
using MediatR;

namespace Coolzo.Application.Features.User.Queries.GetUserById;

public sealed class GetUserByIdQueryHandler : IRequestHandler<GetUserByIdQuery, UserDetailResponse>
{
    private readonly IAuditLogRepository _auditLogRepository;
    private readonly ICustomerPasswordPolicyService _customerPasswordPolicyService;
    private readonly IUserRepository _userRepository;

    public GetUserByIdQueryHandler(
        IUserRepository userRepository,
        ICustomerPasswordPolicyService customerPasswordPolicyService,
        IAuditLogRepository auditLogRepository)
    {
        _userRepository = userRepository;
        _customerPasswordPolicyService = customerPasswordPolicyService;
        _auditLogRepository = auditLogRepository;
    }

    public async Task<UserDetailResponse> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdWithRolesAsync(request.UserId, cancellationToken);

        if (user is null)
        {
            throw new AppException(
                ErrorCodes.NotFound,
                "The user could not be found.",
                404);
        }

        var passwordState = await _customerPasswordPolicyService.GetPasswordStateAsync(user, cancellationToken);
        var activity = await _auditLogRepository.ListRecentUserActivityAsync(user.UserId, user.UserName, 10, cancellationToken);

        return UserResponseMapper.ToDetailResponse(
            user,
            passwordState,
            activity.Select(UserResponseMapper.ToActivityResponse).ToArray());
    }
}
