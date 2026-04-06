using Coolzo.Contracts.Responses.CustomerAuth;
using MediatR;

namespace Coolzo.Application.Features.CustomerAccounts.Commands.CreateCustomerAccount;

public sealed record CreateCustomerAccountCommand(
    string CustomerName,
    string MobileNumber,
    string EmailAddress) : IRequest<CustomerAccountResponse>;
