using Coolzo.Application.Common.Interfaces;
using Coolzo.Domain.Entities;
using Coolzo.Shared.Constants;

namespace Coolzo.Application.Common.Security;

public sealed class CustomerAccountLookupService
{
    private readonly IBookingRepository _bookingRepository;
    private readonly IUserRepository _userRepository;

    public CustomerAccountLookupService(
        IBookingRepository bookingRepository,
        IUserRepository userRepository)
    {
        _bookingRepository = bookingRepository;
        _userRepository = userRepository;
    }

    public bool IsCustomerUser(User? user)
    {
        if (user is null)
        {
            return false;
        }

        var activeRoleNames = user.UserRoles
            .Where(userRole => userRole.Role is not null &&
                userRole.Role.IsActive &&
                !userRole.Role.IsDeleted)
            .Select(userRole => userRole.Role!.RoleName)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        return activeRoleNames.Length == 1 &&
            activeRoleNames.Contains(RoleNames.Customer, StringComparer.OrdinalIgnoreCase);
    }

    public async Task<CustomerAccountLookupResult?> FindByLoginIdAsync(string loginId, CancellationToken cancellationToken)
    {
        var normalizedLoginId = loginId.Trim();
        var user = await _userRepository.GetByUserNameOrEmailAsync(normalizedLoginId, cancellationToken);

        if (IsCustomerUser(user))
        {
            var customerByUser = await _bookingRepository.GetCustomerByUserIdAsync(user!.UserId, cancellationToken);

            if (customerByUser is not null && !customerByUser.IsGuestCustomer)
            {
                return new CustomerAccountLookupResult(customerByUser, user);
            }
        }

        if (!LooksLikeMobileNumber(normalizedLoginId))
        {
            return null;
        }

        var customerByMobile = await _bookingRepository.GetCustomerByMobileAsync(normalizedLoginId, cancellationToken);

        if (customerByMobile?.UserId is not long userId || customerByMobile.IsGuestCustomer)
        {
            return null;
        }

        var customerUser = user is not null && user.UserId == userId
            ? user
            : await _userRepository.GetByIdWithRolesAsync(userId, cancellationToken);

        return IsCustomerUser(customerUser)
            ? new CustomerAccountLookupResult(customerByMobile, customerUser!)
            : null;
    }

    public async Task<CustomerAccountLookupResult?> FindByUserIdAsync(long userId, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdWithRolesAsync(userId, cancellationToken);

        if (!IsCustomerUser(user))
        {
            return null;
        }

        var customer = await _bookingRepository.GetCustomerByUserIdAsync(userId, cancellationToken);

        return customer is not null && !customer.IsGuestCustomer
            ? new CustomerAccountLookupResult(customer, user!)
            : null;
    }

    public async Task<CustomerAccountLookupResult?> FindByCustomerIdAsync(long customerId, CancellationToken cancellationToken)
    {
        var customer = await _bookingRepository.GetCustomerByIdAsync(customerId, cancellationToken);

        if (customer?.UserId is not long userId || customer.IsGuestCustomer)
        {
            return null;
        }

        var user = await _userRepository.GetByIdWithRolesAsync(userId, cancellationToken);

        return IsCustomerUser(user)
            ? new CustomerAccountLookupResult(customer, user!)
            : null;
    }

    private static bool LooksLikeMobileNumber(string loginId)
    {
        var normalized = new string(loginId.Where(char.IsDigit).ToArray());
        return normalized.Length is >= 10 and <= 15;
    }
}

public sealed record CustomerAccountLookupResult(Customer Customer, User User);
