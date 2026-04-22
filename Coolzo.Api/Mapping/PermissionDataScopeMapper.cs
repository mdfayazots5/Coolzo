using Coolzo.Shared.Constants;

namespace Coolzo.Api.Mapping;

public static class PermissionDataScopeMapper
{
    public static string Resolve(IReadOnlyCollection<string> roles)
    {
        if (roles.Count == 0)
        {
            return "OwnOnly";
        }

        if (roles.Any(IsGlobalRole))
        {
            return "All";
        }

        if (roles.Any(IsOwnOnlyRole))
        {
            return "OwnOnly";
        }

        return "BranchOnly";
    }

    private static bool IsGlobalRole(string roleName)
    {
        return roleName.Equals(RoleNames.SuperAdmin, StringComparison.OrdinalIgnoreCase) ||
            roleName.Equals(RoleNames.Admin, StringComparison.OrdinalIgnoreCase) ||
            roleName.Equals("FinanceManager", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsOwnOnlyRole(string roleName)
    {
        return roleName.Equals(RoleNames.Technician, StringComparison.OrdinalIgnoreCase) ||
            roleName.Equals(RoleNames.Helper, StringComparison.OrdinalIgnoreCase) ||
            roleName.Equals(RoleNames.Customer, StringComparison.OrdinalIgnoreCase);
    }
}
