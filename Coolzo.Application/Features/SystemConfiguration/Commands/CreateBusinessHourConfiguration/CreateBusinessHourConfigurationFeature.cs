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

namespace Coolzo.Application.Features.SystemConfiguration.Commands.CreateBusinessHourConfiguration;

public sealed record BusinessHourConfigurationInput(
    int DayOfWeekNumber,
    TimeSpan? StartTimeLocal,
    TimeSpan? EndTimeLocal,
    bool IsClosed);

public sealed record CreateBusinessHourConfigurationCommand(
    IReadOnlyCollection<BusinessHourConfigurationInput> BusinessHours) : IRequest<IReadOnlyCollection<BusinessHourConfigurationResponse>>;

public sealed class CreateBusinessHourConfigurationCommandValidator : AbstractValidator<CreateBusinessHourConfigurationCommand>
{
    public CreateBusinessHourConfigurationCommandValidator()
    {
        RuleFor(request => request.BusinessHours).NotEmpty();
        RuleForEach(request => request.BusinessHours).SetValidator(new BusinessHourConfigurationInputValidator());
        RuleFor(request => request.BusinessHours)
            .Must(inputs => inputs.Select(item => item.DayOfWeekNumber).Distinct().Count() == inputs.Count)
            .WithMessage("Business hours must contain one entry per day.");
    }

    private sealed class BusinessHourConfigurationInputValidator : AbstractValidator<BusinessHourConfigurationInput>
    {
        public BusinessHourConfigurationInputValidator()
        {
            RuleFor(item => item.DayOfWeekNumber).InclusiveBetween(0, 6);
            RuleFor(item => item)
                .Must(item => item.IsClosed || (item.StartTimeLocal.HasValue && item.EndTimeLocal.HasValue && item.StartTimeLocal.Value < item.EndTimeLocal.Value))
                .WithMessage("Open business hours require a valid start and end time.");
        }
    }
}

public sealed class CreateBusinessHourConfigurationCommandHandler : IRequestHandler<CreateBusinessHourConfigurationCommand, IReadOnlyCollection<BusinessHourConfigurationResponse>>
{
    private readonly AdminActivityLogger _adminActivityLogger;
    private readonly IAdminConfigurationRepository _adminConfigurationRepository;
    private readonly ICurrentDateTime _currentDateTime;
    private readonly ICurrentUserContext _currentUserContext;
    private readonly IAppLogger<CreateBusinessHourConfigurationCommandHandler> _logger;
    private readonly IUnitOfWork _unitOfWork;

    public CreateBusinessHourConfigurationCommandHandler(
        IAdminConfigurationRepository adminConfigurationRepository,
        IUnitOfWork unitOfWork,
        ICurrentDateTime currentDateTime,
        ICurrentUserContext currentUserContext,
        AdminActivityLogger adminActivityLogger,
        IAppLogger<CreateBusinessHourConfigurationCommandHandler> logger)
    {
        _adminConfigurationRepository = adminConfigurationRepository;
        _unitOfWork = unitOfWork;
        _currentDateTime = currentDateTime;
        _currentUserContext = currentUserContext;
        _adminActivityLogger = adminActivityLogger;
        _logger = logger;
    }

    public async Task<IReadOnlyCollection<BusinessHourConfigurationResponse>> Handle(CreateBusinessHourConfigurationCommand request, CancellationToken cancellationToken)
    {
        foreach (var item in request.BusinessHours.OrderBy(entry => entry.DayOfWeekNumber))
        {
            var entity = await _adminConfigurationRepository.GetBusinessHourByDayOfWeekAsync(item.DayOfWeekNumber, cancellationToken);

            if (entity is null)
            {
                entity = new BusinessHourConfiguration
                {
                    DayOfWeekNumber = item.DayOfWeekNumber,
                    CreatedBy = _currentUserContext.UserName,
                    DateCreated = _currentDateTime.UtcNow,
                    IPAddress = _currentUserContext.IPAddress
                };

                await _adminConfigurationRepository.AddBusinessHourAsync(entity, cancellationToken);
            }

            entity.IsClosed = item.IsClosed;
            entity.StartTimeLocal = item.IsClosed ? null : item.StartTimeLocal;
            entity.EndTimeLocal = item.IsClosed ? null : item.EndTimeLocal;
            entity.UpdatedBy = _currentUserContext.UserName;
            entity.LastUpdated = _currentDateTime.UtcNow;
            entity.IPAddress = _currentUserContext.IPAddress;
        }

        await _adminActivityLogger.WriteAsync(
            "SaveBusinessHours",
            nameof(BusinessHourConfiguration),
            "WeeklySchedule",
            $"Updated {request.BusinessHours.Count} business hour entries.",
            cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var updated = await _adminConfigurationRepository.GetBusinessHoursAsync(cancellationToken);

        _logger.LogInformation("Business hours updated by {UserName}.", _currentUserContext.UserName);

        return updated.Select(AdminResponseMapper.ToResponse).ToArray();
    }
}
