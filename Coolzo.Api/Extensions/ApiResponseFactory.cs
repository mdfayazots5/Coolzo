using Coolzo.Contracts.Common;
using Coolzo.Shared.Constants;

namespace Coolzo.Api.Extensions;

public static class ApiResponseFactory
{
    public static ApiResponse<T> Success<T>(T data, string traceId, string message = "Request completed successfully.")
    {
        return new ApiResponse<T>(
            true,
            SuccessCodes.Success,
            message,
            data,
            Array.Empty<ApiError>(),
            traceId,
            DateTimeOffset.UtcNow);
    }

    public static ApiResponse<T> Failure<T>(
        string code,
        string message,
        string traceId,
        IReadOnlyCollection<ApiError> errors,
        T? data = default)
    {
        return new ApiResponse<T>(
            false,
            code,
            message,
            data,
            errors,
            traceId,
            DateTimeOffset.UtcNow);
    }
}
