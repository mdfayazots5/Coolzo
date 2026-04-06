using Coolzo.Contracts.Responses.CustomerAuth;
using MediatR;

namespace Coolzo.Application.Features.CustomerAuth.Commands.ForgotCustomerPassword;

public sealed record ForgotCustomerPasswordCommand(string LoginId) : IRequest<CustomerPasswordOperationResponse>;
