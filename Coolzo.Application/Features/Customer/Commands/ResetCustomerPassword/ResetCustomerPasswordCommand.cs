using Coolzo.Contracts.Responses.CustomerAuth;
using MediatR;

namespace Coolzo.Application.Features.CustomerAccounts.Commands.ResetCustomerPassword;

public sealed record ResetCustomerPasswordCommand(
    long CustomerId,
    string? Reason) : IRequest<CustomerPasswordOperationResponse>;
