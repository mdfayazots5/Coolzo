using Coolzo.Application.Common.Interfaces;
using Coolzo.Application.Common.Security;
using Coolzo.Contracts.Responses.CustomerAuth;
using Coolzo.Domain.Entities;
using Coolzo.Domain.Enums;
using Coolzo.Shared.Constants;
using Coolzo.Shared.Exceptions;
using Coolzo.Shared.Models;
using MediatR;

namespace Coolzo.Application.Features.CustomerAccounts.Commands.ResetCustomerPassword;

public sealed class ResetCustomerPasswordCommandHandler : IRequestHandler<ResetCustomerPasswordCommand, CustomerPasswordOperationResponse>
{
    private readonly IAuditLogRepository _auditLogRepository;
    private readonly CustomerAccountLookupService _customerAccountLookupService;
    private readonly ICurrentDateTime _currentDateTime;
    private readonly ICurrentUserContext _currentUserContext;
    private readonly ICustomerPasswordPolicyService _customerPasswordPolicyService;
    private readonly IUnitOfWork _unitOfWork;

    public ResetCustomerPasswordCommandHandler(
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

    public async Task<CustomerPasswordOperationResponse> Handle(ResetCustomerPasswordCommand request, CancellationToken cancellationToken)
    {
        var account = await _customerAccountLookupService.FindByCustomerIdAsync(request.CustomerId, cancellationToken)
            ?? throw new AppException(
                ErrorCodes.NotFound,
                "The customer account could not be found.",
                404);
        var preparedPassword = await _customerPasswordPolicyService.PreparePasswordAsync(
            null,
            CustomerPasswordChangeSource.AdminReset,
            account.User.UserId,
            cancellationToken);
        var actorName = ResolveActorName("AdminCustomerPasswordReset");
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
                request.CustomerId.ToString(),
                preparedPassword,
                actorName,
                ipAddress,
                request.Reason),
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
        string entityId,
        PreparedCustomerPassword preparedPassword,
        string actorName,
        string ipAddress,
        string? reason)
    {
        var newValues = $"Source={preparedPassword.ChangeSource};Mode={preparedPassword.PasswordStorageMode};Generated={preparedPassword.PasswordGenerated}";

        if (!string.IsNullOrWhiteSpace(reason))
        {
            newValues = $"{newValues};Reason={reason.Trim()}";
        }

        return new AuditLog
        {
            UserId = userId,
            ActionName = "AdminResetCustomerPassword",
            EntityName = "Customer",
            EntityId = entityId,
            TraceId = _currentUserContext.TraceId,
            StatusName = "Success",
            NewValues = newValues,
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
