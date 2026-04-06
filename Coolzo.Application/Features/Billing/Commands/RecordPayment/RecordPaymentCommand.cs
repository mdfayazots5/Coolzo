using Coolzo.Contracts.Responses.Billing;
using MediatR;

namespace Coolzo.Application.Features.Billing.Commands.RecordPayment;

public sealed record RecordPaymentCommand(
    long InvoiceId,
    decimal PaidAmount,
    string PaymentMethod,
    string? ReferenceNumber,
    string? Remarks,
    string? IdempotencyKey,
    string? GatewayTransactionId,
    string? Signature,
    decimal? ExpectedInvoiceAmount,
    bool IsWebhookEvent,
    string? WebhookReference) : IRequest<PaymentTransactionResponse>;
