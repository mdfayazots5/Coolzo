using System.Security.Cryptography;
using Coolzo.Application.Common.Interfaces;
using Coolzo.Domain.Entities;
using Coolzo.Domain.Enums;
using Coolzo.Shared.Constants;
using Coolzo.Shared.Exceptions;
using Coolzo.Shared.Models;

namespace Coolzo.Application.Common.Security;

public sealed class CustomerPasswordPolicyService : ICustomerPasswordPolicyService
{
    private const int MaxPasswordLength = 512;
    private const string LowercaseCharacters = "abcdefghijklmnopqrstuvwxyz";
    private const string UppercaseCharacters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
    private const string NumberCharacters = "0123456789";
    private const string SpecialCharacters = "!@#$%^&*()-_=+[]{}?";

    private readonly IApplicationEnvironment _applicationEnvironment;
    private readonly ICurrentDateTime _currentDateTime;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ISystemSettingRepository _systemSettingRepository;
    private readonly IUserPasswordHistoryRepository _userPasswordHistoryRepository;
    private readonly IUserRepository _userRepository;

    public CustomerPasswordPolicyService(
        ISystemSettingRepository systemSettingRepository,
        IPasswordHasher passwordHasher,
        IUserPasswordHistoryRepository userPasswordHistoryRepository,
        IUserRepository userRepository,
        ICurrentDateTime currentDateTime,
        IApplicationEnvironment applicationEnvironment)
    {
        _systemSettingRepository = systemSettingRepository;
        _passwordHasher = passwordHasher;
        _userPasswordHistoryRepository = userPasswordHistoryRepository;
        _userRepository = userRepository;
        _currentDateTime = currentDateTime;
        _applicationEnvironment = applicationEnvironment;
    }

    public async Task<CustomerPasswordPolicySnapshot> ResolvePolicyAsync(CancellationToken cancellationToken)
    {
        var settings = await _systemSettingRepository.GetByKeysAsync(CustomerPasswordSettingKeys.All, cancellationToken);

        var policy = new CustomerPasswordPolicySnapshot(
            GetEnumValue<CustomerPasswordMode>(settings, CustomerPasswordSettingKeys.PasswordMode),
            GetEnumValue<CustomerPasswordGenerationMode>(settings, CustomerPasswordSettingKeys.PasswordGenerationMode),
            GetOptionalStringValue(settings, CustomerPasswordSettingKeys.FixedDefaultPassword),
            GetIntegerValue(settings, CustomerPasswordSettingKeys.PasswordMinLength),
            GetBooleanValue(settings, CustomerPasswordSettingKeys.RequireUppercase),
            GetBooleanValue(settings, CustomerPasswordSettingKeys.RequireLowercase),
            GetBooleanValue(settings, CustomerPasswordSettingKeys.RequireNumber),
            GetBooleanValue(settings, CustomerPasswordSettingKeys.RequireSpecialCharacter),
            GetBooleanValue(settings, CustomerPasswordSettingKeys.ForcePasswordChangeOnFirstLogin),
            GetBooleanValue(settings, CustomerPasswordSettingKeys.AllowAdminResetPassword),
            GetBooleanValue(settings, CustomerPasswordSettingKeys.AllowSelfRegistrationPassword),
            GetBooleanValue(settings, CustomerPasswordSettingKeys.AllowPlainTextModeOnlyInNonProduction),
            GetEnumValue<CustomerPasswordValidationMode>(settings, CustomerPasswordSettingKeys.PasswordValidationMode),
            GetBooleanValue(settings, CustomerPasswordSettingKeys.PasswordHistoryEnabled),
            GetIntegerValue(settings, CustomerPasswordSettingKeys.PasswordExpiryDays));

        ValidatePolicy(policy);

        return policy;
    }

    public async Task<PreparedCustomerPassword> PreparePasswordAsync(
        string? providedPassword,
        CustomerPasswordChangeSource changeSource,
        long? existingUserId,
        CancellationToken cancellationToken)
    {
        var policy = await ResolvePolicyAsync(cancellationToken);
        var useProvidedPassword = ShouldUseProvidedPassword(policy, changeSource, providedPassword);
        var rawPassword = useProvidedPassword
            ? providedPassword!
            : GeneratePassword(policy, changeSource);

        ValidatePasswordAgainstPolicy(rawPassword, policy);

        if (existingUserId.HasValue &&
            changeSource == CustomerPasswordChangeSource.ProfileChange)
        {
            await EnsurePasswordIsNotReusedAsync(existingUserId.Value, rawPassword, policy.PasswordHistoryEnabled, cancellationToken);
        }

        var storedPassword = policy.PasswordMode == CustomerPasswordMode.Hashed
            ? _passwordHasher.HashPassword(rawPassword)
            : rawPassword;
        var passwordGenerated = !useProvidedPassword;
        var requiresPasswordDelivery = passwordGenerated;
        var isTemporaryPassword = changeSource != CustomerPasswordChangeSource.ProfileChange;
        var mustChangePassword = changeSource != CustomerPasswordChangeSource.ProfileChange &&
            policy.ForcePasswordChangeOnFirstLogin;
        DateTime? passwordExpiryOnUtc = policy.PasswordExpiryDays > 0
            ? _currentDateTime.UtcNow.AddDays(policy.PasswordExpiryDays)
            : null;

        return new PreparedCustomerPassword(
            policy,
            rawPassword,
            storedPassword,
            policy.PasswordMode,
            changeSource,
            passwordGenerated,
            requiresPasswordDelivery,
            mustChangePassword,
            isTemporaryPassword,
            passwordExpiryOnUtc);
    }

    public async Task<bool> VerifyPasswordAsync(User user, string providedPassword, CancellationToken cancellationToken)
    {
        var policy = await ResolvePolicyAsync(cancellationToken);

        return policy.PasswordValidationMode switch
        {
            CustomerPasswordValidationMode.ForceHashValidation => VerifyHashedPassword(user.PasswordHash, providedPassword),
            CustomerPasswordValidationMode.ForcePlainTextValidation => string.Equals(user.PasswordHash, providedPassword, StringComparison.Ordinal),
            _ => MatchesStoredPassword(user.PasswordHash, providedPassword, user.PasswordStorageMode)
        };
    }

    public Task<CustomerPasswordStateSnapshot> GetPasswordStateAsync(User user, CancellationToken cancellationToken)
    {
        _ = cancellationToken;

        var isExpired = user.PasswordExpiryOnUtc.HasValue && user.PasswordExpiryOnUtc.Value <= _currentDateTime.UtcNow;

        return Task.FromResult(
            new CustomerPasswordStateSnapshot(
                user.MustChangePassword || isExpired,
                user.IsTemporaryPassword,
                user.PasswordExpiryOnUtc,
                isExpired));
    }

    public async Task ApplyPasswordAsync(
        User user,
        PreparedCustomerPassword preparedPassword,
        string actorName,
        string ipAddress,
        CancellationToken cancellationToken)
    {
        var changedOnUtc = _currentDateTime.UtcNow;

        user.PasswordHash = preparedPassword.StoredPassword;
        user.PasswordStorageMode = preparedPassword.PasswordStorageMode;
        user.MustChangePassword = preparedPassword.MustChangePassword;
        user.PasswordLastChangedOnUtc = changedOnUtc;
        user.PasswordExpiryOnUtc = preparedPassword.PasswordExpiryOnUtc;
        user.PasswordUpdatedBy = actorName;
        user.IsTemporaryPassword = preparedPassword.IsTemporaryPassword;
        user.LastUpdated = changedOnUtc;
        user.UpdatedBy = actorName;

        if (preparedPassword.ChangeSource == CustomerPasswordChangeSource.ProfileChange)
        {
            user.PasswordResetSource = null;
        }
        else
        {
            user.LastPasswordResetOnUtc = changedOnUtc;
            user.PasswordResetSource = preparedPassword.ChangeSource;
        }

        await _userPasswordHistoryRepository.AddAsync(
            new UserPasswordHistory
            {
                User = user,
                PasswordValue = preparedPassword.StoredPassword,
                PasswordStorageMode = preparedPassword.PasswordStorageMode,
                ChangeSource = preparedPassword.ChangeSource,
                ChangedOnUtc = changedOnUtc,
                CreatedBy = actorName,
                DateCreated = changedOnUtc,
                IPAddress = ipAddress
            },
            cancellationToken);
    }

    private void ValidatePolicy(CustomerPasswordPolicySnapshot policy)
    {
        if (policy.PasswordMinLength < 1 || policy.PasswordMinLength > 128)
        {
            throw CreateValidationException("Password minimum length must be between 1 and 128 characters.");
        }

        if (policy.PasswordExpiryDays < 0 || policy.PasswordExpiryDays > 3650)
        {
            throw CreateValidationException("Password expiry days must be between 0 and 3650.");
        }

        if (policy.PasswordMode == CustomerPasswordMode.PlainText &&
            policy.AllowPlainTextModeOnlyInNonProduction &&
            _applicationEnvironment.IsProduction())
        {
            throw CreateValidationException("Plain text customer password mode is blocked in the current production environment.");
        }

        if (policy.PasswordMode == CustomerPasswordMode.PlainText &&
            policy.PasswordValidationMode == CustomerPasswordValidationMode.ForceHashValidation)
        {
            throw CreateValidationException("Customer password validation mode cannot force hash validation while customer password mode is plain text.");
        }

        if (policy.PasswordMode == CustomerPasswordMode.Hashed &&
            policy.PasswordValidationMode == CustomerPasswordValidationMode.ForcePlainTextValidation)
        {
            throw CreateValidationException("Customer password validation mode cannot force plain-text validation while customer password mode is hashed.");
        }

        if (policy.PasswordGenerationMode == CustomerPasswordGenerationMode.Fixed)
        {
            if (string.IsNullOrWhiteSpace(policy.FixedDefaultPassword))
            {
                throw CreateValidationException("A fixed default customer password is required when fixed password generation mode is enabled.");
            }

            ValidatePasswordAgainstPolicy(policy.FixedDefaultPassword, policy);
        }
    }

    private async Task EnsurePasswordIsNotReusedAsync(
        long existingUserId,
        string newPassword,
        bool passwordHistoryEnabled,
        CancellationToken cancellationToken)
    {
        var existingUser = await _userRepository.GetByIdWithRolesAsync(existingUserId, cancellationToken);

        if (existingUser is not null &&
            MatchesStoredPassword(existingUser.PasswordHash, newPassword, existingUser.PasswordStorageMode))
        {
            throw CreateValidationException("The new password must be different from the current password.");
        }

        if (!passwordHistoryEnabled)
        {
            return;
        }

        var passwordHistory = await _userPasswordHistoryRepository.ListByUserIdAsync(existingUserId, cancellationToken);

        if (passwordHistory.Any(history => MatchesStoredPassword(history.PasswordValue, newPassword, history.PasswordStorageMode)))
        {
            throw CreateValidationException("The new password matches a previously used password.");
        }
    }

    private static bool ShouldUseProvidedPassword(
        CustomerPasswordPolicySnapshot policy,
        CustomerPasswordChangeSource changeSource,
        string? providedPassword)
    {
        return changeSource switch
        {
            CustomerPasswordChangeSource.SelfRegistration => policy.AllowSelfRegistrationPassword && !string.IsNullOrWhiteSpace(providedPassword),
            CustomerPasswordChangeSource.ProfileChange => !string.IsNullOrWhiteSpace(providedPassword),
            _ => false
        };
    }

    private string GeneratePassword(CustomerPasswordPolicySnapshot policy, CustomerPasswordChangeSource changeSource)
    {
        if (changeSource == CustomerPasswordChangeSource.AdminReset && !policy.AllowAdminResetPassword)
        {
            throw new AppException(
                ErrorCodes.FeatureDisabled,
                "Admin customer password reset is disabled by system configuration.",
                409);
        }

        return policy.PasswordGenerationMode switch
        {
            CustomerPasswordGenerationMode.Fixed => policy.FixedDefaultPassword!,
            _ => GenerateRandomPassword(policy)
        };
    }

    private static void ValidatePasswordAgainstPolicy(string password, CustomerPasswordPolicySnapshot policy)
    {
        var validationErrors = new List<(string Code, string Message)>();

        if (string.IsNullOrWhiteSpace(password))
        {
            validationErrors.Add((nameof(password), "Password is required."));
        }

        if (password.Length < policy.PasswordMinLength)
        {
            validationErrors.Add((nameof(password), $"Password must be at least {policy.PasswordMinLength} characters long."));
        }

        if (password.Length > MaxPasswordLength)
        {
            validationErrors.Add((nameof(password), $"Password must not exceed {MaxPasswordLength} characters."));
        }

        if (policy.RequireUppercase && !password.Any(char.IsUpper))
        {
            validationErrors.Add((nameof(password), "Password must include at least one uppercase letter."));
        }

        if (policy.RequireLowercase && !password.Any(char.IsLower))
        {
            validationErrors.Add((nameof(password), "Password must include at least one lowercase letter."));
        }

        if (policy.RequireNumber && !password.Any(char.IsDigit))
        {
            validationErrors.Add((nameof(password), "Password must include at least one number."));
        }

        if (policy.RequireSpecialCharacter && password.All(character => !SpecialCharacters.Contains(character)))
        {
            validationErrors.Add((nameof(password), "Password must include at least one special character."));
        }

        if (validationErrors.Count > 0)
        {
            throw new AppException(
                ErrorCodes.ValidationFailure,
                "The password does not satisfy the configured customer password policy.",
                400,
                validationErrors);
        }
    }

    private bool MatchesStoredPassword(string storedPassword, string providedPassword, CustomerPasswordMode storedMode)
    {
        return storedMode switch
        {
            CustomerPasswordMode.PlainText => string.Equals(storedPassword, providedPassword, StringComparison.Ordinal),
            _ => VerifyHashedPassword(storedPassword, providedPassword)
        };
    }

    private bool VerifyHashedPassword(string storedPassword, string providedPassword)
    {
        if (string.IsNullOrWhiteSpace(storedPassword))
        {
            return false;
        }

        return _passwordHasher.VerifyPassword(storedPassword, providedPassword);
    }

    private static string GenerateRandomPassword(CustomerPasswordPolicySnapshot policy)
    {
        var requiredCharacterSets = new List<string>();
        var allowedCharacterSets = new List<string>();

        if (policy.RequireLowercase)
        {
            requiredCharacterSets.Add(LowercaseCharacters);
        }

        if (policy.RequireUppercase)
        {
            requiredCharacterSets.Add(UppercaseCharacters);
        }

        if (policy.RequireNumber)
        {
            requiredCharacterSets.Add(NumberCharacters);
        }

        if (policy.RequireSpecialCharacter)
        {
            requiredCharacterSets.Add(SpecialCharacters);
        }

        if (policy.RequireLowercase || (!policy.RequireUppercase && !policy.RequireNumber && !policy.RequireSpecialCharacter))
        {
            allowedCharacterSets.Add(LowercaseCharacters);
        }

        if (policy.RequireUppercase)
        {
            allowedCharacterSets.Add(UppercaseCharacters);
        }

        if (policy.RequireNumber)
        {
            allowedCharacterSets.Add(NumberCharacters);
        }

        if (policy.RequireSpecialCharacter)
        {
            allowedCharacterSets.Add(SpecialCharacters);
        }

        if (allowedCharacterSets.Count == 0)
        {
            allowedCharacterSets.Add(LowercaseCharacters);
            allowedCharacterSets.Add(UppercaseCharacters);
            allowedCharacterSets.Add(NumberCharacters);
        }

        var passwordLength = Math.Max(policy.PasswordMinLength, requiredCharacterSets.Count);
        var passwordCharacters = new List<char>(passwordLength);

        foreach (var requiredSet in requiredCharacterSets)
        {
            passwordCharacters.Add(requiredSet[RandomNumberGenerator.GetInt32(requiredSet.Length)]);
        }

        var combinedCharacterPool = string.Concat(allowedCharacterSets.Distinct());

        while (passwordCharacters.Count < passwordLength)
        {
            passwordCharacters.Add(combinedCharacterPool[RandomNumberGenerator.GetInt32(combinedCharacterPool.Length)]);
        }

        for (var index = passwordCharacters.Count - 1; index > 0; index--)
        {
            var swapIndex = RandomNumberGenerator.GetInt32(index + 1);
            (passwordCharacters[index], passwordCharacters[swapIndex]) = (passwordCharacters[swapIndex], passwordCharacters[index]);
        }

        return new string(passwordCharacters.ToArray());
    }

    private static TEnum GetEnumValue<TEnum>(
        IReadOnlyDictionary<string, SystemSetting> settings,
        string key)
        where TEnum : struct, Enum
    {
        var rawValue = GetRequiredStringValue(settings, key);

        if (Enum.TryParse<TEnum>(rawValue, true, out var enumValue))
        {
            return enumValue;
        }

        throw CreateValidationException($"System setting '{key}' has an invalid value '{rawValue}'.");
    }

    private static bool GetBooleanValue(
        IReadOnlyDictionary<string, SystemSetting> settings,
        string key)
    {
        var rawValue = GetRequiredStringValue(settings, key);

        if (bool.TryParse(rawValue, out var booleanValue))
        {
            return booleanValue;
        }

        throw CreateValidationException($"System setting '{key}' must be either true or false.");
    }

    private static int GetIntegerValue(
        IReadOnlyDictionary<string, SystemSetting> settings,
        string key)
    {
        var rawValue = GetRequiredStringValue(settings, key);

        if (int.TryParse(rawValue, out var integerValue))
        {
            return integerValue;
        }

        throw CreateValidationException($"System setting '{key}' must contain a valid integer value.");
    }

    private static string? GetOptionalStringValue(
        IReadOnlyDictionary<string, SystemSetting> settings,
        string key)
    {
        return settings.TryGetValue(key, out var setting)
            ? setting.SettingValue?.Trim()
            : null;
    }

    private static string GetRequiredStringValue(
        IReadOnlyDictionary<string, SystemSetting> settings,
        string key)
    {
        if (settings.TryGetValue(key, out var setting) && !string.IsNullOrWhiteSpace(setting.SettingValue))
        {
            return setting.SettingValue.Trim();
        }

        throw CreateValidationException($"Required system setting '{key}' is missing or empty.");
    }

    private static AppException CreateValidationException(string message)
    {
        return new AppException(
            ErrorCodes.ValidationFailure,
            message,
            400);
    }
}
