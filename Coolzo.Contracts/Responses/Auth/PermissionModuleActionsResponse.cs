namespace Coolzo.Contracts.Responses.Auth;

public sealed record PermissionModuleActionsResponse(
    bool View,
    bool Create,
    bool Edit,
    bool Delete,
    bool Approve,
    bool Export);
