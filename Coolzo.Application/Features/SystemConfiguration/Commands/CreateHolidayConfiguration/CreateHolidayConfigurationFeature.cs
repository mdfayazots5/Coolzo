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

namespace Coolzo.Application.Features.SystemConfiguration.Commands.CreateHolidayConfiguration;

public sealed record CreateHolidayConfigurationCommand(
    DateOnly HolidayDate,
    string HolidayName,
    bool IsRecurringAnnually,
    bool IsActive) : IRequest<HolidayConfigurationResponse>;

public sealed class CreateHolidayConfigurationCommandValidator : AbstractValidator<CreateHolidayConfigurationCommand>
{
    public CreateHolidayConfigurationCommandValidator()
    {
        RuleFor(request => request.HolidayName).NotEmpty().MaximumLength(128);
    }
}

public sealed class CreateHolidayConfigurationCommandHandler : IRequestHandler<CreateHolidayConfigurationCommand, HolidayConfigurationResponse>
{
    private readonly AdminActivityLogger _adminActivityLogger;
    private readonly IAdminConfigurationRepository _adminConfigurationRepository;
    private readonly ICurrentDateTime _currentDateTime;
    private readonly ICurrentUserContext _currentUserContext;
    private readonly IAppLogger<CreateHolidayConfigurationCommandHandler> _logger;
    private readonly IUnitOfWork _unitOfWork;

    public CreateHolidayConfigurationCommandHandler(
        IAdminConfigurationRepository adminConfigurationRepository,
        IUnitOfWork unitOfWork,
        ICurrentDateTime currentDateTime,
        ICurrentUserContext currentUserContext,
        AdminActivityLogger adminActivityLogger,
        IAppLogger<CreateHolidayConfigurationCommandHandler> logger)
    {
        _adminConfigurationRepository = adminConfigurationRepository;
        _unitOfWork = unitOfWork;
        _currentDateTime = currentDateTime;
        _currentUserContext = currentUserContext;
        _adminActivityLogger = adminActivityLogger;
        _logger = logger;
    }

    public async Task<HolidayConfigurationResponse> Handle(CreateHolidayConfigurationCommand request, CancellationToken cancellationToken)
    {
        var duplicate = await _adminConfigurationRepository.GetHolidayByDateAsync(request.HolidayDate, null, cancellationToken);

        if (duplicate is not null)
        {
            throw new AppException(ErrorCodes.DuplicateValue, "A holiday already exists for the requested date.", 409);
        }

        var entity = new HolidayConfiguration
        {
            HolidayDate = request.HolidayDate,
            HolidayName = request.HolidayName.Trim(),
            IsRecurringAnnually = request.IsRecurringAnnually,
            IsActive = request.IsActive,
            CreatedBy = _currentUserContext.UserName,
            DateCreated = _currentDateTime.UtcNow,
            IPAddress = _currentUserContext.IPAddress
        };

        await _adminConfigurationRepository.AddHolidayAsync(entity, cancellationToken);
        await _adminActivityLogger.WriteAsync(
            "CreateHolidayConfiguration",
            nameof(HolidayConfiguration),
            entity.HolidayDate.ToString("yyyy-MM-dd"),
            entity.HolidayName,
            cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Holiday {HolidayDate} created by {UserName}.", entity.HolidayDate, _currentUserContext.UserName);

        return AdminResponseMapper.ToResponse(entity);
    }
}
