using Coolzo.Application.Common.Interfaces;
using Coolzo.Contracts.Responses.Auth;
using Coolzo.Domain.Entities;
using Coolzo.Shared.Constants;

namespace Coolzo.Application.Common.Security;

public sealed class AuthenticatedUserProfileFactory
{
    private readonly IBookingRepository _bookingRepository;
    private readonly CustomerAccountLookupService _customerAccountLookupService;
    private readonly ICustomerPasswordPolicyService _customerPasswordPolicyService;
    private readonly IGapPhaseERepository _gapPhaseERepository;
    private readonly ITechnicianRepository _technicianRepository;

    public AuthenticatedUserProfileFactory(
        ITechnicianRepository technicianRepository,
        IGapPhaseERepository gapPhaseERepository,
        IBookingRepository bookingRepository,
        CustomerAccountLookupService customerAccountLookupService,
        ICustomerPasswordPolicyService customerPasswordPolicyService)
    {
        _technicianRepository = technicianRepository;
        _gapPhaseERepository = gapPhaseERepository;
        _bookingRepository = bookingRepository;
        _customerAccountLookupService = customerAccountLookupService;
        _customerPasswordPolicyService = customerPasswordPolicyService;
    }

    public async Task<AuthenticatedUserProfileSnapshot> CreateAsync(User user, CancellationToken cancellationToken)
    {
        var roles = user.UserRoles
            .Where(userRole => userRole.Role is not null && userRole.Role.IsActive && !userRole.Role.IsDeleted)
            .Select(userRole => userRole.Role!.RoleName)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
        var permissions = user.UserRoles
            .Where(userRole => userRole.Role is not null && !userRole.Role.IsDeleted)
            .SelectMany(userRole => userRole.Role!.RolePermissions)
            .Where(rolePermission => rolePermission.Permission is not null && rolePermission.Permission.IsActive && !rolePermission.Permission.IsDeleted)
            .Select(rolePermission => rolePermission.Permission!.PermissionName)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
        var technicianId = (await _technicianRepository.GetByUserIdAsync(user.UserId, cancellationToken))?.TechnicianId;
        var helperProfileId = (await _gapPhaseERepository.GetHelperProfileByUserIdAsync(user.UserId, cancellationToken))?.HelperProfileId;
        long? customerId = null;
        var mustChangePassword = false;
        var isTemporaryPassword = false;
        DateTime? passwordExpiryOnUtc = null;

        if (_customerAccountLookupService.IsCustomerUser(user))
        {
            customerId = (await _bookingRepository.GetCustomerByUserIdAsync(user.UserId, cancellationToken))?.CustomerId;

            var passwordState = await _customerPasswordPolicyService.GetPasswordStateAsync(user, cancellationToken);
            mustChangePassword = passwordState.MustChangePassword;
            isTemporaryPassword = passwordState.IsTemporaryPassword;
            passwordExpiryOnUtc = passwordState.PasswordExpiryOnUtc;
        }

        return new AuthenticatedUserProfileSnapshot(
            roles,
            permissions,
            new CurrentUserResponse(
                user.UserId,
                user.UserName,
                user.Email,
                user.FullName,
                technicianId,
                helperProfileId,
                user.BranchId,
                roles,
                permissions,
                customerId,
                mustChangePassword,
                isTemporaryPassword,
                passwordExpiryOnUtc));
    }
}

public sealed record AuthenticatedUserProfileSnapshot(
    IReadOnlyCollection<string> Roles,
    IReadOnlyCollection<string> Permissions,
    CurrentUserResponse CurrentUser);
