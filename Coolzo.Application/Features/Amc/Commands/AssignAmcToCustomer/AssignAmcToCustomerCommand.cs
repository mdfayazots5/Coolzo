using Coolzo.Contracts.Responses.Amc;
using MediatR;

namespace Coolzo.Application.Features.Amc.Commands.AssignAmcToCustomer;

public sealed record AssignAmcToCustomerCommand(
    long CustomerId,
    long AmcPlanId,
    long JobCardId,
    long InvoiceId,
    DateTime? StartDateUtc,
    string? Remarks) : IRequest<CustomerAmcResponse>;
