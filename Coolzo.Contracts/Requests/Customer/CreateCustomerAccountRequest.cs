namespace Coolzo.Contracts.Requests.Customer;

public sealed record CreateCustomerAccountRequest(
    string CustomerName,
    string MobileNumber,
    string EmailAddress);
