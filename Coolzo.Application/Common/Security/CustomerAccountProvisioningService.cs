using Coolzo.Application.Common.Interfaces;
using Coolzo.Domain.Entities;
using Coolzo.Domain.Enums;
using Coolzo.Shared.Constants;
using Coolzo.Shared.Exceptions;
using Coolzo.Shared.Models;

namespace Coolzo.Application.Common.Security;

public sealed class CustomerAccountProvisioningService
{
    private readonly IAuditLogRepository _auditLogRepository;
    private readonly IBookingRepository _bookingRepository;
    private readonly ICurrentDateTime _currentDateTime;
    private readonly ICurrentUserContext _currentUserContext;
    private readonly ICustomerPasswordPolicyService _customerPasswordPolicyService;
    private readonly IRoleRepository _roleRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUserRepository _userRepository;

    public CustomerAccountProvisioningService(
        IBookingRepository bookingRepository,
        IUserRepository userRepository,
        IRoleRepository roleRepository,
        ICustomerPasswordPolicyService customerPasswordPolicyService,
        IAuditLogRepository auditLogRepository,
        IUnitOfWork unitOfWork,
        ICurrentDateTime currentDateTime,
        ICurrentUserContext currentUserContext)
    {
        _bookingRepository = bookingRepository;
        _userRepository = userRepository;
        _roleRepository = roleRepository;
        _customerPasswordPolicyService = customerPasswordPolicyService;
        _auditLogRepository = auditLogRepository;
        _unitOfWork = unitOfWork;
        _currentDateTime = currentDateTime;
        _currentUserContext = currentUserContext;
    }

    public async Task<CustomerAccountProvisioningResult> ProvisionAsync(
        CustomerAccountProvisioningRequest request,
        CustomerPasswordChangeSource passwordChangeSource,
        string auditActionName,
        CancellationToken cancellationToken)
    {
        var normalizedCustomerName = request.CustomerName.Trim();
        var normalizedMobileNumber = request.MobileNumber.Trim();
        var normalizedEmailAddress = request.EmailAddress.Trim();
        var now = _currentDateTime.UtcNow;
        var actorName = ResolveActorName(auditActionName);
        var ipAddress = ResolveIpAddress();

        var existingCustomer = await _bookingRepository.GetCustomerByMobileAsync(normalizedMobileNumber, cancellationToken);

        if (existingCustomer is not null && existingCustomer.UserId.HasValue)
        {
            throw new AppException(
                ErrorCodes.DuplicateValue,
                "A registered customer account already exists for the provided mobile number.",
                409);
        }

        if (await _userRepository.ExistsByUserNameAsync(normalizedMobileNumber, null, cancellationToken))
        {
            throw new AppException(
                ErrorCodes.DuplicateValue,
                "The provided mobile number is already assigned to another login.",
                409);
        }

        if (await _userRepository.ExistsByEmailAsync(normalizedEmailAddress, null, cancellationToken))
        {
            throw new AppException(
                ErrorCodes.DuplicateValue,
                "The provided email address is already assigned to another login.",
                409);
        }

        var customerRole = await _roleRepository.GetByNameAsync(RoleNames.Customer, cancellationToken)
            ?? throw new AppException(
                ErrorCodes.NotFound,
                "The customer role could not be found.",
                404);
        var preparedPassword = await _customerPasswordPolicyService.PreparePasswordAsync(
            request.Password,
            passwordChangeSource,
            null,
            cancellationToken);
        var user = new User
        {
            UserName = normalizedMobileNumber,
            Email = normalizedEmailAddress,
            FullName = normalizedCustomerName,
            IsActive = true,
            CreatedBy = actorName,
            DateCreated = now,
            IPAddress = ipAddress
        };

        await _customerPasswordPolicyService.ApplyPasswordAsync(
            user,
            preparedPassword,
            actorName,
            ipAddress,
            cancellationToken);

        user.UserRoles.Add(
            new UserRole
            {
                RoleId = customerRole.RoleId,
                CreatedBy = actorName,
                DateCreated = now,
                IPAddress = ipAddress
            });

        await _userRepository.AddAsync(user, cancellationToken);

        Customer customer;

        if (existingCustomer is null)
        {
            customer = new Customer
            {
                User = user,
                CustomerName = normalizedCustomerName,
                MobileNumber = normalizedMobileNumber,
                EmailAddress = normalizedEmailAddress,
                IsGuestCustomer = false,
                IsActive = true,
                CreatedBy = actorName,
                DateCreated = now,
                IPAddress = ipAddress
            };

            await _bookingRepository.AddCustomerAsync(customer, cancellationToken);
        }
        else
        {
            customer = existingCustomer;
            customer.User = user;
            customer.CustomerName = normalizedCustomerName;
            customer.MobileNumber = normalizedMobileNumber;
            customer.EmailAddress = normalizedEmailAddress;
            customer.IsGuestCustomer = false;
            customer.IsActive = true;
            customer.LastUpdated = now;
            customer.UpdatedBy = actorName;
        }

        await _auditLogRepository.AddAsync(
            new AuditLog
            {
                UserId = _currentUserContext.UserId,
                ActionName = auditActionName,
                EntityName = "Customer",
                EntityId = normalizedMobileNumber,
                TraceId = _currentUserContext.TraceId,
                StatusName = "Success",
                NewValues = $"Source={passwordChangeSource};Mode={preparedPassword.PasswordStorageMode};Generated={preparedPassword.PasswordGenerated}",
                CreatedBy = actorName,
                DateCreated = now,
                IPAddress = ipAddress
            },
            cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new CustomerAccountProvisioningResult(customer, user, preparedPassword);
    }

    private string ResolveActorName(string fallbackActorName)
    {
        return string.IsNullOrWhiteSpace(_currentUserContext.UserName)
            ? fallbackActorName
            : _currentUserContext.UserName;
    }

    private string ResolveIpAddress()
    {
        return string.IsNullOrWhiteSpace(_currentUserContext.IPAddress)
            ? "127.0.0.1"
            : _currentUserContext.IPAddress;
    }
}

public sealed record CustomerAccountProvisioningRequest(
    string CustomerName,
    string MobileNumber,
    string EmailAddress,
    string? Password);

public sealed record CustomerAccountProvisioningResult(
    Customer Customer,
    User User,
    PreparedCustomerPassword PreparedPassword);
