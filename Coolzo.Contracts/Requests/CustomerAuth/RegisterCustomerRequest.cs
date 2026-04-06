namespace Coolzo.Contracts.Requests.CustomerAuth;

public sealed record RegisterCustomerRequest(
    string CustomerName,
    string MobileNumber,
    string EmailAddress,
    string? Password);
