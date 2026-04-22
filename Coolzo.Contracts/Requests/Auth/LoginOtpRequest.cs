namespace Coolzo.Contracts.Requests.Auth;

public sealed record LoginOtpRequest(string LoginId, string Otp);
