namespace Coolzo.Contracts.Requests.Auth;

public sealed record VerifyCustomerOtpRequest(string Phone, string Otp);
