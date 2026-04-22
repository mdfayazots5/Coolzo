namespace Coolzo.Contracts.Requests.Auth;

public sealed record VerifyOtpRequest(string Email, string Otp);
