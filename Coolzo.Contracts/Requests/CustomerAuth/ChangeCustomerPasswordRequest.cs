namespace Coolzo.Contracts.Requests.CustomerAuth;

public sealed record ChangeCustomerPasswordRequest(
    string CurrentPassword,
    string NewPassword);
