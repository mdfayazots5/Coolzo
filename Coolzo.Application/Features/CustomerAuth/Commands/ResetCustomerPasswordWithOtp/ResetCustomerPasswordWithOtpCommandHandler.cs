using Coolzo.Application.Common.Interfaces;
using Coolzo.Application.Common.Security;
using Coolzo.Contracts.Responses.CustomerAuth;
using Coolzo.Domain.Entities;
using Coolzo.Domain.Enums;
using Coolzo.Shared.Constants;
using Coolzo.Shared.Exceptions;
using Coolzo.Shared.Models;
using MediatR;

namespace Coolzo.Application.Features.CustomerAuth.Commands.ResetCustomerPasswordWithOtp;

public sealed class ResetCustomerPasswordWithOtpCommandHandler : IRequestHandler<ResetCustomerPasswordWithOtpCommand, CustomerPasswordOperationResponse>
{
    private const string OtpPurpose = "LoginOtp";

    private readonly IAuditLogRepository _auditLogRepository;
    private readonly CustomerAccountLookupService _customerAccountLookupService;
    private readonly ICurrentDateTime _currentDateTime;
    private readonly ICurrentUserContext _currentUserContext;
    private readonly ICustomerPasswordPolicyService _customerPasswordPolicyService;
    private readonly IOtpVerificationRepository _otpVerificationRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ResetCustomerPasswordWithOtpCommandHandler(
        CustomerAccountLookupService customerAccountLookupService,
        ICustomerPasswordPolicyService customerPasswordPolicyService,
        IOtpVerificationRepository otpVerificationRepository,
        IAuditLogRepository auditLogRepository,
        IUnitOfWork unitOfWork,
        ICurrentDateTime currentDateTime,
        ICurrentUserContext currentUserContext)
    {
        _customerAccountLookupService = customerAccountLookupService;
        _customerPasswordPolicyService = customerPasswordPolicyService;
        _otpVerificationRepository = otpVerificationRepository;
        _auditLogRepository = auditLogRepository;
        _unitOfWork = unitOfWork;
        _currentDateTime = currentDateTime;
        _currentUserContext = currentUserContext;
    }

    public async Task<CustomerPasswordOperationResponse> Handle(ResetCustomerPasswordWithOtpCommand request, CancellationToken cancellationToken)
    {
        var account = await _customerAccountLookupService.FindByLoginIdAsync(request.LoginId.Trim(), cancellationToken)
            ?? throw new AppException(ErrorCodes.InvalidCredentials, "The verification code is invalid or expired.", 401);
        var otp = await _otpVerificationRepository.GetActiveByUserAndCodeAsync(
                account.User.UserId,
                OtpPurpose,
                request.Otp.Trim(),
                _currentDateTime.UtcNow,
                cancellationToken)
            ?? throw new AppException(ErrorCodes.InvalidCredentials, "The verification code is invalid or expired.", 401);
        var preparedPassword = await _customerPasswordPolicyService.PreparePasswordAsync(
            request.NewPassword,
            CustomerPasswordChangeSource.ForgotPassword,
            account.User.UserId,
            cancellationToken);
        var actorName = ResolveActorName("CustomerResetPasswordWithOtp");
        var ipAddress = ResolveIpAddress();

        otp.IsConsumed = true;
        otp.ConsumedAtUtc = _currentDateTime.UtcNow;
        otp.UpdatedBy = actorName;
        otp.LastUpdated = _currentDateTime.UtcNow;

        await _customerPasswordPolicyService.ApplyPasswordAsync(
            account.User,
            preparedPassword,
            actorName,
            ipAddress,
            cancellationToken);
        await _auditLogRepository.AddAsync(
            CreateAuditLog(
                account.User.UserId,
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
        string entityId,
        PreparedCustomerPassword preparedPassword,
        string actorName,
        string ipAddress)
    {
        return new AuditLog
        {
            UserId = userId,
            ActionName = "CustomerResetPasswordWithOtp",
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
