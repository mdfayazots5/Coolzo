using Coolzo.Application.Common.Interfaces;
using Coolzo.Application.Common.Services;
using Coolzo.Contracts.Responses.CMS;
using Coolzo.Domain.Entities;
using Coolzo.Shared.Constants;
using Coolzo.Shared.Exceptions;
using Coolzo.Shared.Models;
using FluentValidation;
using MediatR;

namespace Coolzo.Application.Features.CMS.Theme.Commands.UpdateTheme;

public sealed record UpdateThemeCommand(IReadOnlyDictionary<string, string> Tokens) : IRequest<ThemeResponse>;

public sealed class UpdateThemeCommandValidator : AbstractValidator<UpdateThemeCommand>
{
    public UpdateThemeCommandValidator()
    {
        RuleFor(request => request.Tokens).NotEmpty();
    }
}

public sealed class UpdateThemeCommandHandler : IRequestHandler<UpdateThemeCommand, ThemeResponse>
{
    private const string ThemeDataType = "string";

    private readonly AdminActivityLogger _adminActivityLogger;
    private readonly ICurrentDateTime _currentDateTime;
    private readonly ICurrentUserContext _currentUserContext;
    private readonly IAppLogger<UpdateThemeCommandHandler> _logger;
    private readonly ISystemSettingRepository _systemSettingRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateThemeCommandHandler(
        ISystemSettingRepository systemSettingRepository,
        IUnitOfWork unitOfWork,
        ICurrentDateTime currentDateTime,
        ICurrentUserContext currentUserContext,
        AdminActivityLogger adminActivityLogger,
        IAppLogger<UpdateThemeCommandHandler> logger)
    {
        _systemSettingRepository = systemSettingRepository;
        _unitOfWork = unitOfWork;
        _currentDateTime = currentDateTime;
        _currentUserContext = currentUserContext;
        _adminActivityLogger = adminActivityLogger;
        _logger = logger;
    }

    public async Task<ThemeResponse> Handle(UpdateThemeCommand request, CancellationToken cancellationToken)
    {
        var invalidKeys = request.Tokens.Keys
            .Where(key => !SnapshotKeys.Theme.AllKeys.Contains(key))
            .ToArray();

        if (invalidKeys.Length > 0)
        {
            throw new AppException(
                ErrorCodes.ValidationFailure,
                $"Unknown theme token keys: {string.Join(", ", invalidKeys)}.",
                400);
        }

        var now = _currentDateTime.UtcNow;

        foreach (var (key, value) in request.Tokens)
        {
            var existing = await _systemSettingRepository.GetTrackedByKeyAsync(key, cancellationToken);

            if (existing is null)
            {
                await _systemSettingRepository.AddAsync(
                    new SystemSetting
                    {
                        SettingKey = key,
                        SettingValue = value ?? string.Empty,
                        DataType = ThemeDataType,
                        IsSensitive = false,
                        CreatedBy = _currentUserContext.UserName,
                        DateCreated = now,
                        IPAddress = _currentUserContext.IPAddress
                    },
                    cancellationToken);
            }
            else
            {
                existing.SettingValue = value ?? string.Empty;
                existing.UpdatedBy = _currentUserContext.UserName;
                existing.LastUpdated = now;
            }
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _adminActivityLogger.WriteAsync(
            "UpdateTheme",
            nameof(SystemSetting),
            "theme",
            $"Updated {request.Tokens.Count} theme token(s).",
            cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Theme updated ({Count} tokens) by {UserName}.", request.Tokens.Count, _currentUserContext.UserName);

        var stored = await _systemSettingRepository.GetByKeysAsync(SnapshotKeys.Theme.AllKeys, cancellationToken);
        var tokens = SnapshotKeys.Theme.AllKeys.ToDictionary(
            key => key,
            key => stored.TryGetValue(key, out var setting) ? setting.SettingValue : string.Empty);

        return new ThemeResponse(tokens);
    }
}
