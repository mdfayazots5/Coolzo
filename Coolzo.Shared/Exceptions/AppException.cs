namespace Coolzo.Shared.Exceptions;

public sealed class AppException : Exception
{
    public AppException(string code, string message, int statusCode, IReadOnlyCollection<(string Code, string Message)>? errors = null)
        : base(message)
    {
        Code = code;
        StatusCode = statusCode;
        Errors = errors ?? Array.Empty<(string Code, string Message)>();
    }

    public string Code { get; }

    public int StatusCode { get; }

    public IReadOnlyCollection<(string Code, string Message)> Errors { get; }
}
