using Coolzo.Application.Common.Interfaces;
using Coolzo.Application.Common.Mappings;
using Coolzo.Application.Common.Services;
using Coolzo.Contracts.Responses.Admin;
using Coolzo.Domain.Entities;
using Coolzo.Shared.Constants;
using Coolzo.Shared.Exceptions;
using Coolzo.Shared.Models;
using FluentValidation;
using MediatR;

namespace Coolzo.Application.Features.CommunicationPreference.Commands.CreateOrUpdateCommunicationPreference;

public sealed record CreateOrUpdateCommunicationPreferenceCommand(
    bool EmailEnabled,
    bool SmsEnabled,
    bool WhatsAppEnabled,
    bool PushEnabled,
    bool AllowPromotionalContent,
    string? EmailAddress,
    string? MobileNumber) : IRequest<CommunicationPreferenceResponse>;

public sealed class CreateOrUpdateCommunicationPreferenceCommandValidator : AbstractValidator<CreateOrUpdateCommunicationPreferenceCommand>
{
    public CreateOrUpdateCommunicationPreferenceCommandValidator()
    {
        RuleFor(request => request.EmailAddress).EmailAddress().MaximumLength(128).When(request => !string.IsNullOrWhiteSpace(request.EmailAddress));
        RuleFor(request => request.MobileNumber).Matches("^[0-9]{0,16}$").When(request => !string.IsNullOrWhiteSpace(request.MobileNumber));
        RuleFor(request => request)
            .Must(request => !request.EmailEnabled || !string.IsNullOrWhiteSpace(request.EmailAddress))
            .WithMessage("Email-enabled preferences require an email address.");
        RuleFor(request => request)
            .Must(request => (!request.SmsEnabled && !request.WhatsAppEnabled) || !string.IsNullOrWhiteSpace(request.MobileNumber))
            .WithMessage("SMS or WhatsApp preferences require a mobile number.");
    }
}

public sealed class CreateOrUpdateCommunicationPreferenceCommandHandler : IRequestHandler<CreateOrUpdateCommunicationPreferenceCommand, CommunicationPreferenceResponse>
{
    private readonly AdminActivityLogger _adminActivityLogger;
    private readonly IAdminConfigurationRepository _adminConfigurationRepository;
    private readonly IBookingRepository _bookingRepository;
    private readonly ICurrentDateTime _currentDateTime;
    private readonly ICurrentUserContext _currentUserContext;
    private readonly IAppLogger<CreateOrUpdateCommunicationPreferenceCommandHandler> _logger;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUserRepository _userRepository;

    public CreateOrUpdateCommunicationPreferenceCommandHandler(
        IAdminConfigurationRepository adminConfigurationRepository,
        IBookingRepository bookingRepository,
        IUserRepository userRepository,
        IUnitOfWork unitOfWork,
        ICurrentDateTime currentDateTime,
        ICurrentUserContext currentUserContext,
        AdminActivityLogger adminActivityLogger,
        IAppLogger<CreateOrUpdateCommunicationPreferenceCommandHandler> logger)
    {
        _adminConfigurationRepository = adminConfigurationRepository;
        _bookingRepository = bookingRepository;
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
        _currentDateTime = currentDateTime;
        _currentUserContext = currentUserContext;
        _adminActivityLogger = adminActivityLogger;
        _logger = logger;
    }

    public async Task<CommunicationPreferenceResponse> Handle(CreateOrUpdateCommunicationPreferenceCommand request, CancellationToken cancellationToken)
    {
        if (!_currentUserContext.IsAuthenticated || !_currentUserContext.UserId.HasValue)
        {
            throw new AppException(ErrorCodes.Unauthorized, "Customer authentication is required.", 401);
        }

        var customer = await ResolveCustomerAsync(request, cancellationToken);
        var entity = await _adminConfigurationRepository.GetCommunicationPreferenceByCustomerIdAsync(customer.CustomerId, cancellationToken);

        if (entity is null)
        {
            entity = new Domain.Entities.CommunicationPreference
            {
                Customer = customer.CustomerId == 0 ? customer : null,
                CustomerId = customer.CustomerId,
                CreatedBy = _currentUserContext.UserName,
                DateCreated = _currentDateTime.UtcNow,
                IPAddress = _currentUserContext.IPAddress
            };

            await _adminConfigurationRepository.AddCommunicationPreferenceAsync(entity, cancellationToken);
        }

        entity.EmailAddress = request.EmailAddress?.Trim() ?? customer.EmailAddress;
        entity.MobileNumber = request.MobileNumber?.Trim() ?? customer.MobileNumber;
        entity.EmailEnabled = request.EmailEnabled;
        entity.SmsEnabled = request.SmsEnabled;
        entity.WhatsAppEnabled = request.WhatsAppEnabled;
        entity.PushEnabled = request.PushEnabled;
        entity.AllowPromotionalContent = request.AllowPromotionalContent;
        entity.UpdatedBy = _currentUserContext.UserName;
        entity.LastUpdated = _currentDateTime.UtcNow;
        entity.IPAddress = _currentUserContext.IPAddress;

        await _adminActivityLogger.WriteAsync(
            "CreateOrUpdateCommunicationPreference",
            nameof(Domain.Entities.CommunicationPreference),
            customer.CustomerId == 0 ? _currentUserContext.UserId.Value.ToString() : customer.CustomerId.ToString(),
            "Communication preference updated.",
            cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Communication preferences updated by {UserName}.", _currentUserContext.UserName);

        return AdminResponseMapper.ToResponse(entity);
    }

    private async Task<Customer> ResolveCustomerAsync(CreateOrUpdateCommunicationPreferenceCommand request, CancellationToken cancellationToken)
    {
        var customer = await _bookingRepository.GetCustomerByUserIdAsync(_currentUserContext.UserId!.Value, cancellationToken);

        if (customer is not null)
        {
            if (!string.IsNullOrWhiteSpace(request.EmailAddress))
            {
                customer.EmailAddress = request.EmailAddress.Trim();
            }

            if (!string.IsNullOrWhiteSpace(request.MobileNumber))
            {
                customer.MobileNumber = request.MobileNumber.Trim();
            }

            customer.LastUpdated = _currentDateTime.UtcNow;
            customer.UpdatedBy = _currentUserContext.UserName;
            customer.IPAddress = _currentUserContext.IPAddress;

            return customer;
        }

        var user = await _userRepository.GetByIdWithRolesAsync(_currentUserContext.UserId.Value, cancellationToken)
            ?? throw new AppException(ErrorCodes.NotFound, "The authenticated user could not be resolved.", 404);

        customer = new Customer
        {
            UserId = user.UserId,
            CustomerName = user.FullName,
            MobileNumber = request.MobileNumber?.Trim() ?? string.Empty,
            EmailAddress = request.EmailAddress?.Trim() ?? user.Email,
            IsGuestCustomer = false,
            IsActive = true,
            CreatedBy = _currentUserContext.UserName,
            DateCreated = _currentDateTime.UtcNow,
            IPAddress = _currentUserContext.IPAddress
        };

        await _bookingRepository.AddCustomerAsync(customer, cancellationToken);

        return customer;
    }
}
