using Coolzo.Application.Common.Security;
using Coolzo.Contracts.Responses.User;

namespace Coolzo.Application.Features.User;

internal static class UserResponseMapper
{
    public static UserResponse ToResponse(Coolzo.Domain.Entities.User user)
    {
        var roles = GetActiveRoles(user);

        return new UserResponse(
            user.UserId,
            user.UserName,
            user.Email,
            user.FullName,
            user.IsActive,
            user.BranchId,
            roles.Select(role => role.RoleId).ToArray(),
            roles.Select(role => role.DisplayName).ToArray(),
            user.DateCreated,
            user.LastLoginDateUtc,
            user.MustChangePassword);
    }

    public static UserDetailResponse ToDetailResponse(
        Coolzo.Domain.Entities.User user,
        CustomerPasswordStateSnapshot passwordState,
        IReadOnlyCollection<UserActivityResponse> recentActivity)
    {
        var roles = GetActiveRoles(user);
        var permissions = roles
            .SelectMany(
                role => role.RolePermissions
                    .Where(rolePermission =>
                        !rolePermission.IsDeleted &&
                        rolePermission.Permission is not null &&
                        rolePermission.Permission.IsActive &&
                        !rolePermission.Permission.IsDeleted)
                    .Select(rolePermission => rolePermission.Permission!.PermissionName))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(permission => permission)
            .ToArray();

        return new UserDetailResponse(
            user.UserId,
            user.UserName,
            user.Email,
            user.FullName,
            user.IsActive,
            user.BranchId,
            roles.Select(role => role.RoleId).ToArray(),
            roles.Select(role => role.DisplayName).ToArray(),
            permissions,
            user.DateCreated,
            user.LastUpdated,
            user.LastLoginDateUtc,
            passwordState.MustChangePassword,
            passwordState.IsTemporaryPassword,
            passwordState.PasswordExpiryOnUtc,
            recentActivity);
    }

    public static UserActivityResponse ToActivityResponse(Coolzo.Domain.Entities.AuditLog auditLog)
    {
        var description = auditLog.NewValues;

        if (string.IsNullOrWhiteSpace(description))
        {
            description = auditLog.OldValues;
        }

        if (string.IsNullOrWhiteSpace(description))
        {
            description = $"{auditLog.ActionName} recorded.";
        }

        return new UserActivityResponse(
            auditLog.AuditLogId.ToString(),
            auditLog.ActionName,
            auditLog.StatusName,
            string.IsNullOrWhiteSpace(auditLog.CreatedBy) ? "System" : auditLog.CreatedBy,
            description,
            auditLog.DateCreated);
    }

    private static Coolzo.Domain.Entities.Role[] GetActiveRoles(Coolzo.Domain.Entities.User user)
    {
        return user.UserRoles
            .Where(userRole => !userRole.IsDeleted && userRole.Role is not null && userRole.Role.IsActive && !userRole.Role.IsDeleted)
            .Select(userRole => userRole.Role!)
            .OrderBy(role => role.DisplayName)
            .ToArray();
    }
}
