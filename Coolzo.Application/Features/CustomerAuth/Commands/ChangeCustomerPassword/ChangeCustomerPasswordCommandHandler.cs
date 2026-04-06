using Coolzo.Application.Common.Interfaces;
using Coolzo.Application.Common.Security;
using Coolzo.Contracts.Responses.CustomerAuth;
using Coolzo.Domain.Entities;
using Coolzo.Domain.Enums;
using Coolzo.Shared.Constants;
using Coolzo.Shared.Exceptions;
using Coolzo.Shared.Models;
using MediatR;

namespace Coolzo.Application.Features.CustomerAuth.Commands.ChangeCustomerPassword;

public sealed class ChangeCustomerPasswordCommandHandler : IRequestHandler<ChangeCustomerPasswordCommand, CustomerPasswordOperationResponse>
{
    private readonly IAuditLogRepository _auditLogRepository;
    private readonly CustomerAccountLookupService _customerAccountLookupService;
    private readonly ICurrentDateTime _currentDateTime;
    private readonly ICurrentUserContext _currentUserContext;
    private readonly ICustomerPasswordPolicyService _customerPasswordPolicyService;
    private readonly IUnitOfWork _unitOfWork;

    public ChangeCustomerPasswordCommandHandler(
        CustomerAccountLookupService customerAccountLookupService,
        ICustomerPasswordPolicyService customerPasswordPolicyService,
        IAuditLogRepository auditLogRepository,
        IUnitOfWork unitOfWork,
        ICurrentDateTime currentDateTime,
        ICurrentUserContext currentUserContext)
    {
        _customerAccountLookupService = customerAccountLookupService;
        _customerPasswordPolicyService = customerPasswordPolicyService;
        _auditLogRepository = auditLogRepository;
        _unitOfWork = unitOfWork;
        _currentDateTime = currentDateTime;
        _currentUserContext = currentUserContext;
    }

    public async Task<CustomerPasswordOperationResponse> Handle(ChangeCustomerPasswordCommand request, CancellationToken cancellationToken)
    {
        if (!_currentUserContext.IsAuthenticated || !_currentUserContext.UserId.HasValue)
        {
            throw new AppException(ErrorCodes.Unauthorized, "Customer authentication is required.", 401);
        }

        var account = await _customerAccountLookupService.FindByUserIdAsync(_currentUserContext.UserId.Value, cancellationToken)
            ?? throw new AppException(ErrorCodes.Unauthorized, "Customer authentication is required.", 401);

        if (!await _customerPasswordPolicyService.VerifyPasswordAsync(account.User, request.CurrentPassword, cancellationToken))
        {
            throw new AppException(ErrorCodes.InvalidCredentials, "The current password is incorrect.", 401);
        }

        var preparedPassword = await _customerPasswordPolicyService.PreparePasswordAsync(
            request.NewPassword,
            CustomerPasswordChangeSource.ProfileChange,
            account.User.UserId,
            cancellationToken);
        var actorName = ResolveActorName("CustomerPasswordChange");
        var ipAddress = ResolveIpAddress();

        await _customerPasswordPolicyService.ApplyPasswordAsync(
            account.User,
            preparedPassword,
            actorName,
            ipAddress,
            cancellationToken);
        await _auditLogRepository.AddAsync(
            CreateAuditLog(
                account.User.UserId,
                "CustomerChangePassword",
                account.Customer.CustomerId.ToString(),
                preparedPassword,
                actorName,
                ipAddress),
            cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new CustomerPasswordOperationResponse(
            true,
            preparedPassword.PasswordGenerated,
            preparedPassword.RequiresPasswordDelivery,
            preparedPassword.MustChangePassword,
            preparedPassword.IsTemporaryPassword,
            preparedPassword.PasswordExpiryOnUtc);
    }

    private AuditLog CreateAuditLog(
        long userId,
        string actionName,
        string entityId,
        PreparedCustomerPassword preparedPassword,
        string actorName,
        string ipAddress)
    {
        return new AuditLog
        {
            UserId = userId,
            ActionName = actionName,
            EntityName = "Customer",
            EntityId = entityId,
            TraceId = _currentUserContext.TraceId,
            StatusName = "Success",
            NewValues = $"Source={preparedPassword.ChangeSource};Mode={preparedPassword.PasswordStorageMode}",
            CreatedBy = actorName,
            DateCreated = _currentDateTime.UtcNow,
            IPAddress = ipAddress
        };
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
