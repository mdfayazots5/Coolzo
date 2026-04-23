namespace Coolzo.Contracts.Responses.Billing;

public sealed record PaymentGatewaySessionResponse(
    string PaymentId,
    string PaymentUrl,
    string Status);
