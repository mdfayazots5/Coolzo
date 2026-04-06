namespace Coolzo.Domain.Enums;

public enum CustomerPasswordValidationMode
{
    MatchStoredMode = 1,
    ForceHashValidation = 2,
    ForcePlainTextValidation = 3
}
