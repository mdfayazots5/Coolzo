using Coolzo.Contracts.Responses.CustomerAuth;
using MediatR;

namespace Coolzo.Application.Features.CustomerAuth.Commands.RegisterCustomer;

public sealed record RegisterCustomerCommand(
    string CustomerName,
    string MobileNumber,
    string EmailAddress,
    string? Password) : IRequest<CustomerAccountResponse>;
