using Coolzo.Contracts.Responses.Auth;
using Coolzo.Shared.Constants;

namespace Coolzo.Api.Mapping;

internal static class PermissionModuleMatrixMapper
{
    private static readonly string[] KnownModules =
    [
        "dashboard",
        "operations",
        "service-requests",
        "scheduling",
        "jobs",
        "attendance",
        "amc",
        "equipment",
        "inventory",
        "billing",
        "finance",
        "support",
        "team",
        "customers",
        "marketing",
        "reports",
        "settings",
        "profile"
    ];

    public static IReadOnlyDictionary<string, PermissionModuleActionsResponse> Build(
        IEnumerable<string> permissionNames,
        IEnumerable<string>? roleNames = null)
    {
        var matrix = KnownModules.ToDictionary(
            module => module,
            _ => new HashSet<string>(StringComparer.OrdinalIgnoreCase),
            StringComparer.OrdinalIgnoreCase);

        var roles = (roleNames ?? Array.Empty<string>())
            .Select(NormalizeKey)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        if (roles.Contains(NormalizeKey(RoleNames.SuperAdmin)))
        {
            foreach (var module in KnownModules)
            {
                Add(matrix, module, "view", "create", "edit", "delete", "approve", "export");
            }

            return ToResponse(matrix);
        }

        foreach (var permissionName in permissionNames.Where(name => !string.IsNullOrWhiteSpace(name)))
        {
            ApplyPermission(matrix, permissionName.Trim());
        }

        var isFieldRole = roles.Contains(NormalizeKey(RoleNames.Technician)) || roles.Contains(NormalizeKey(RoleNames.Helper));

        if (isFieldRole)
        {
            Clear(matrix);
            Add(matrix, "jobs", "view", "edit");
            Add(matrix, "profile", "view", "edit");

            return ToResponse(matrix);
        }

        if (roles.Count > 0)
        {
            Add(matrix, "profile", "view", "edit");
        }

        return ToResponse(matrix);
    }

    private static IReadOnlyDictionary<string, PermissionModuleActionsResponse> ToResponse(
        Dictionary<string, HashSet<string>> matrix)
    {
        return KnownModules.ToDictionary(
            module => module,
            module =>
            {
                var actions = matrix[module];
                return new PermissionModuleActionsResponse(
                    actions.Contains("view"),
                    actions.Contains("create"),
                    actions.Contains("edit"),
                    actions.Contains("delete"),
                    actions.Contains("approve"),
                    actions.Contains("export"));
            },
            StringComparer.OrdinalIgnoreCase);
    }

    private static void ApplyPermission(Dictionary<string, HashSet<string>> matrix, string permissionName)
    {
        var parts = permissionName.Split('.', 2, StringSplitOptions.TrimEntries);
        var scope = parts[0];
        var verb = parts.Length > 1 ? parts[1] : "read";
        var verbActions = ActionsForVerb(verb);

        switch (scope)
        {
            case "dashboard":
                Add(matrix, "dashboard", "view");
                break;
            case "operationsDashboard":
                Add(matrix, "operations", "view");
                break;
            case "booking":
            case "serviceRequest":
                Add(matrix, "service-requests", verbActions);
                break;
            case "assignment":
                Add(matrix, "service-requests", "edit", "approve");
                Add(matrix, "operations", "view", "edit");
                Add(matrix, "scheduling", "view", "edit");
                break;
            case "technician":
                Add(matrix, "team", "view");
                if (IsMutatingVerb(verb))
                {
                    Add(matrix, "team", "edit");
                }

                break;
            case "analytics":
            case "report":
                Add(matrix, "reports", "view", "export");
                break;
            case "user":
            case "role":
            case "permission":
            case "lookup":
            case "configuration":
            case "health":
            case "notificationTemplate":
            case "notificationTrigger":
            case "communicationPreference":
                Add(matrix, "settings", verbActions);
                break;
            case "cms":
                Add(matrix, "settings", verbActions);
                Add(matrix, "marketing", verbActions);
                break;
            case "quotation":
                Add(matrix, "billing", "view");
                if (verb.Equals("create", StringComparison.OrdinalIgnoreCase))
                {
                    Add(matrix, "billing", "create");
                }

                if (verb.Equals("approve", StringComparison.OrdinalIgnoreCase))
                {
                    Add(matrix, "billing", "approve");
                }

                break;
            case "invoice":
            case "billing":
                Add(matrix, "billing", verbActions);
                break;
            case "payment":
                Add(matrix, "finance", "view");
                if (verb.Equals("collect", StringComparison.OrdinalIgnoreCase))
                {
                    Add(matrix, "finance", "edit", "approve");
                }

                break;
            case "amc":
                Add(matrix, "amc", "view");
                if (verb.Equals("create", StringComparison.OrdinalIgnoreCase))
                {
                    Add(matrix, "amc", "create");
                }

                if (verb.Equals("assign", StringComparison.OrdinalIgnoreCase))
                {
                    Add(matrix, "amc", "create", "edit");
                }

                break;
            case "warranty":
            case "revisit":
            case "serviceHistory":
                Add(matrix, "amc", "view");
                Add(matrix, "equipment", "view");
                if (verb.Equals("claim", StringComparison.OrdinalIgnoreCase) ||
                    verb.Equals("create", StringComparison.OrdinalIgnoreCase))
                {
                    Add(matrix, "amc", "create");
                }

                break;
            case "item":
            case "warehouse":
            case "stock":
            case "jobConsumption":
                Add(matrix, "inventory", verbActions);
                if (verb.Equals("manage", StringComparison.OrdinalIgnoreCase))
                {
                    Add(matrix, "inventory", "approve");
                }

                break;
            case "support":
                Add(matrix, "support", "view");
                if (verb.Equals("manage", StringComparison.OrdinalIgnoreCase))
                {
                    Add(matrix, "support", "create", "edit", "approve");
                }

                break;
        }
    }

    private static string[] ActionsForVerb(string verb)
    {
        return verb.ToLowerInvariant() switch
        {
            "read" => ["view"],
            "create" => ["create"],
            "update" => ["edit"],
            "manage" => ["edit"],
            "approve" => ["approve"],
            "collect" => ["edit", "approve"],
            "assign" => ["edit"],
            "claim" => ["create"],
            _ => []
        };
    }

    private static bool IsMutatingVerb(string verb)
    {
        return verb.Equals("update", StringComparison.OrdinalIgnoreCase) ||
            verb.Equals("manage", StringComparison.OrdinalIgnoreCase) ||
            verb.Equals("create", StringComparison.OrdinalIgnoreCase);
    }

    private static void Add(Dictionary<string, HashSet<string>> matrix, string module, params string[] actions)
    {
        if (!matrix.TryGetValue(module, out var existingActions))
        {
            existingActions = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            matrix[module] = existingActions;
        }

        foreach (var action in actions.Where(action => !string.IsNullOrWhiteSpace(action)))
        {
            existingActions.Add(action);
        }
    }

    private static void Clear(Dictionary<string, HashSet<string>> matrix)
    {
        foreach (var actions in matrix.Values)
        {
            actions.Clear();
        }
    }

    private static string NormalizeKey(string value)
    {
        return new string(value.Where(char.IsLetterOrDigit).ToArray()).ToLowerInvariant();
    }
}
