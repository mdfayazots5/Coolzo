namespace Coolzo.Shared.Models;

public class Result<T>
{
    private Result(bool isSuccess, string code, string message, T? data)
    {
        IsSuccess = isSuccess;
        Code = code;
        Message = message;
        Data = data;
    }

    public bool IsSuccess { get; }

    public string Code { get; }

    public string Message { get; }

    public T? Data { get; }

    public static Result<T> Success(T data, string message = "Request completed successfully.")
    {
        return new Result<T>(true, "success", message, data);
    }

    public static Result<T> Failure(string code, string message)
    {
        return new Result<T>(false, code, message, default);
    }
}
