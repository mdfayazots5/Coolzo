using Coolzo.Contracts.Responses.CustomerAuth;
using MediatR;

namespace Coolzo.Application.Features.CustomerAuth.Commands.ResetCustomerPasswordWithOtp;

public sealed record ResetCustomerPasswordWithOtpCommand(
    string LoginId,
    string Otp,
    string NewPassword) : IRequest<CustomerPasswordOperationResponse>;
