namespace Coolzo.Contracts.Responses.Configuration;

public sealed record SystemSettingResponse
(
    long SystemSettingId,
    string SettingKey,
    string SettingValue,
    string DataType,
    bool IsSensitive
);
