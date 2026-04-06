using Coolzo.Contracts.Responses.CustomerAuth;
using MediatR;

namespace Coolzo.Application.Features.CustomerAuth.Commands.ChangeCustomerPassword;

public sealed record ChangeCustomerPasswordCommand(
    string CurrentPassword,
    string NewPassword) : IRequest<CustomerPasswordOperationResponse>;
