namespace Coolzo.Contracts.Responses.Auth;

public sealed record AuthActionResponse
(
    bool Success,
    string Message
);
