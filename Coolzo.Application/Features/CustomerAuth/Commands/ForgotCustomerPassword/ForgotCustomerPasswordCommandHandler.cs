using Coolzo.Application.Common.Interfaces;
using Coolzo.Application.Common.Security;
using Coolzo.Contracts.Responses.CustomerAuth;
using Coolzo.Domain.Entities;
using Coolzo.Domain.Enums;
using Coolzo.Shared.Constants;
using Coolzo.Shared.Exceptions;
using Coolzo.Shared.Models;
using MediatR;

namespace Coolzo.Application.Features.CustomerAuth.Commands.ForgotCustomerPassword;

public sealed class ForgotCustomerPasswordCommandHandler : IRequestHandler<ForgotCustomerPasswordCommand, CustomerPasswordOperationResponse>
{
    private readonly IApplicationEnvironment _applicationEnvironment;
    private readonly IAuditLogRepository _auditLogRepository;
    private readonly CustomerAccountLookupService _customerAccountLookupService;
    private readonly ICurrentDateTime _currentDateTime;
    private readonly ICurrentUserContext _currentUserContext;
    private readonly ICustomerPasswordPolicyService _customerPasswordPolicyService;
    private readonly IUnitOfWork _unitOfWork;

    public ForgotCustomerPasswordCommandHandler(
        IApplicationEnvironment applicationEnvironment,
        CustomerAccountLookupService customerAccountLookupService,
        ICustomerPasswordPolicyService customerPasswordPolicyService,
        IAuditLogRepository auditLogRepository,
        IUnitOfWork unitOfWork,
        ICurrentDateTime currentDateTime,
        ICurrentUserContext currentUserContext)
    {
        _applicationEnvironment = applicationEnvironment;
        _customerAccountLookupService = customerAccountLookupService;
        _customerPasswordPolicyService = customerPasswordPolicyService;
        _auditLogRepository = auditLogRepository;
        _unitOfWork = unitOfWork;
        _currentDateTime = currentDateTime;
        _currentUserContext = currentUserContext;
    }

    public async Task<CustomerPasswordOperationResponse> Handle(ForgotCustomerPasswordCommand request, CancellationToken cancellationToken)
    {
        if (_applicationEnvironment.IsProduction())
        {
            throw new AppException(
                ErrorCodes.FeatureDisabled,
                "Self-service customer password reset is blocked in production until a verified identity flow is enabled.",
                409);
        }

        var account = await _customerAccountLookupService.FindByLoginIdAsync(request.LoginId, cancellationToken)
            ?? throw new AppException(
                ErrorCodes.NotFound,
                "The customer account could not be found.",
                404);
        var preparedPassword = await _customerPasswordPolicyService.PreparePasswordAsync(
            null,
            CustomerPasswordChangeSource.ForgotPassword,
            account.User.UserId,
            cancellationToken);
        var actorName = ResolveActorName("CustomerForgotPassword");
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
                "CustomerForgotPassword",
                account.Customer.CustomerId.ToString(),
                preparedPassword,
                actorName,
                ipAddress),
            cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return CreateResponse(preparedPassword);
    }

    private CustomerPasswordOperationResponse CreateResponse(PreparedCustomerPassword preparedPassword)
    {
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
            NewValues = $"Source={preparedPassword.ChangeSource};Mode={preparedPassword.PasswordStorageMode};Generated={preparedPassword.PasswordGenerated}",
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
