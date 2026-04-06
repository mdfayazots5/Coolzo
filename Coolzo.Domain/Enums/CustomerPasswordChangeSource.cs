namespace Coolzo.Domain.Enums;

public enum CustomerPasswordChangeSource
{
    SelfRegistration = 1,
    AdminCreate = 2,
    ForgotPassword = 3,
    AdminReset = 4,
    ProfileChange = 5
}
