namespace Coolzo.Contracts.Common;

public sealed record ApiResponse<T>
(
    bool IsSuccess,
    string Code,
    string Message,
    T? Data,
    IReadOnlyCollection<ApiError> Errors,
    string TraceId,
    DateTimeOffset TimestampUtc
);
