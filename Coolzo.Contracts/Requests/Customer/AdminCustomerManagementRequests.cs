namespace Coolzo.Contracts.Requests.Customer;

public sealed record UpdateAdminCustomerRequest(
    string CustomerName,
    string MobileNumber,
    string EmailAddress);

public sealed record CreateCustomerNoteRequest(
    string Content,
    bool IsPrivate,
    string? NoteType);
