using Coolzo.Application.Common.Security;
using Coolzo.Contracts.Responses.CustomerAuth;
using Coolzo.Domain.Enums;
using MediatR;

namespace Coolzo.Application.Features.CustomerAccounts.Commands.CreateCustomerAccount;

public sealed class CreateCustomerAccountCommandHandler : IRequestHandler<CreateCustomerAccountCommand, CustomerAccountResponse>
{
    private readonly CustomerAccountProvisioningService _customerAccountProvisioningService;

    public CreateCustomerAccountCommandHandler(CustomerAccountProvisioningService customerAccountProvisioningService)
    {
        _customerAccountProvisioningService = customerAccountProvisioningService;
    }

    public async Task<CustomerAccountResponse> Handle(CreateCustomerAccountCommand request, CancellationToken cancellationToken)
    {
        var result = await _customerAccountProvisioningService.ProvisionAsync(
            new CustomerAccountProvisioningRequest(
                request.CustomerName,
                request.MobileNumber,
                request.EmailAddress,
                null),
            CustomerPasswordChangeSource.AdminCreate,
            "CreateCustomerAccount",
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
