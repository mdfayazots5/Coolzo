using Coolzo.Application.Common.Security;
using Coolzo.Contracts.Responses.CustomerAuth;
using Coolzo.Domain.Enums;
using MediatR;

namespace Coolzo.Application.Features.CustomerAuth.Commands.RegisterCustomer;

public sealed class RegisterCustomerCommandHandler : IRequestHandler<RegisterCustomerCommand, CustomerAccountResponse>
{
    private readonly CustomerAccountProvisioningService _customerAccountProvisioningService;

    public RegisterCustomerCommandHandler(CustomerAccountProvisioningService customerAccountProvisioningService)
    {
        _customerAccountProvisioningService = customerAccountProvisioningService;
    }

    public async Task<CustomerAccountResponse> Handle(RegisterCustomerCommand request, CancellationToken cancellationToken)
    {
        var result = await _customerAccountProvisioningService.ProvisionAsync(
            new CustomerAccountProvisioningRequest(
                request.CustomerName,
                request.MobileNumber,
                request.EmailAddress,
                request.Password),
            CustomerPasswordChangeSource.SelfRegistration,
            "CustomerSelfRegister",
            cancellationToken);

        return new CustomerAccountResponse(
            result.Customer.CustomerId,
            result.User.UserId,
            result.Customer.CustomerName,
            result.Customer.MobileNumber,
            result.Customer.EmailAddress,
            result.PreparedPassword.PasswordGenerated,
            result.PreparedPassword.RequiresPasswordDelivery,
            result.PreparedPassword.MustChangePassword,
            result.PreparedPassword.IsTemporaryPassword,
            result.PreparedPassword.PasswordExpiryOnUtc);
    }
}
