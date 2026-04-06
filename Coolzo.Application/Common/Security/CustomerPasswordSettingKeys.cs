namespace Coolzo.Application.Common.Security;

public static class CustomerPasswordSettingKeys
{
    public const string PasswordMode = "Auth.CustomerPasswordMode";
    public const string PasswordGenerationMode = "Auth.CustomerPasswordGenerationMode";
    public const string FixedDefaultPassword = "Auth.CustomerFixedDefaultPassword";
    public const string PasswordMinLength = "Auth.CustomerPasswordMinLength";
    public const string RequireUppercase = "Auth.CustomerPasswordRequireUppercase";
    public const string RequireLowercase = "Auth.CustomerPasswordRequireLowercase";
    public const string RequireNumber = "Auth.CustomerPasswordRequireNumber";
    public const string RequireSpecialCharacter = "Auth.CustomerPasswordRequireSpecialCharacter";
    public const string ForcePasswordChangeOnFirstLogin = "Auth.CustomerForcePasswordChangeOnFirstLogin";
    public const string AllowAdminResetPassword = "Auth.CustomerAllowAdminResetPassword";
    public const string AllowSelfRegistrationPassword = "Auth.CustomerAllowSelfRegistrationPassword";
    public const string AllowPlainTextModeOnlyInNonProduction = "Auth.CustomerAllowPlainTextModeOnlyInNonProduction";
    public const string PasswordValidationMode = "Auth.CustomerPasswordValidationMode";
    public const string PasswordHistoryEnabled = "Auth.CustomerPasswordHistoryEnabled";
    public const string PasswordExpiryDays = "Auth.CustomerPasswordExpiryDays";

    public static IReadOnlyCollection<string> All =>
    [
        PasswordMode,
        PasswordGenerationMode,
        FixedDefaultPassword,
        PasswordMinLength,
        RequireUppercase,
        RequireLowercase,
        RequireNumber,
        RequireSpecialCharacter,
        ForcePasswordChangeOnFirstLogin,
        AllowAdminResetPassword,
        AllowSelfRegistrationPassword,
        AllowPlainTextModeOnlyInNonProduction,
        PasswordValidationMode,
        PasswordHistoryEnabled,
        PasswordExpiryDays
    ];
}
