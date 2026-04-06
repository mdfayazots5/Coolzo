using System.Net;
using Coolzo.Application.Common.Interfaces;
using Coolzo.Api.Extensions;
using Coolzo.Contracts.Common;
using Coolzo.Domain.Entities;
using Coolzo.Domain.Enums;
using Coolzo.Shared.Constants;
using Coolzo.Shared.Exceptions;
using Coolzo.Shared.Models;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace Coolzo.Api.Middleware;

public sealed class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger, IServiceScopeFactory serviceScopeFactory)
    {
        _next = next;
        _logger = logger;
        _serviceScopeFactory = serviceScopeFactory;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (ValidationException validationException)
        {
            _logger.LogWarning(validationException, "Validation failure while processing request.");

            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            await WriteResponseAsync(
                context,
                ApiResponseFactory.Failure<object?>(
                    ErrorCodes.ValidationFailure,
                    "One or more validation errors occurred.",
                    context.TraceIdentifier,
                    validationException.Errors
                        .Select(error => new ApiError(error.PropertyName, error.ErrorMessage))
                        .ToArray()));
        }
        catch (AppException appException)
        {
            _logger.LogWarning(appException, "Application exception while processing request.");

            context.Response.StatusCode = appException.StatusCode;
            await WriteResponseAsync(
                context,
                ApiResponseFactory.Failure<object?>(
                    appException.Code,
                    appException.Message,
                    context.TraceIdentifier,
                    appException.Errors.Select(error => new ApiError(error.Code, error.Message)).ToArray()));
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Unhandled exception while processing request.");
            await TrackUnhandledExceptionAsync(context, exception);

            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            await WriteResponseAsync(
                context,
                ApiResponseFactory.Failure<object?>(
                    ErrorCodes.UnexpectedError,
                    "An unexpected error occurred.",
                    context.TraceIdentifier,
                    new[] { new ApiError(ErrorCodes.UnexpectedError, "An unexpected error occurred.") }));
        }
    }

    private static Task WriteResponseAsync<T>(HttpContext context, ApiResponse<T> response)
    {
        context.Response.ContentType = "application/json";

        return context.Response.WriteAsJsonAsync(response);
    }

    private async Task TrackUnhandledExceptionAsync(HttpContext context, Exception exception)
    {
        try
        {
            using var scope = _serviceScopeFactory.CreateScope();
            var repository = scope.ServiceProvider.GetRequiredService<IGapPhaseARepository>();
            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            var currentDateTime = scope.ServiceProvider.GetRequiredService<ICurrentDateTime>();

            var alertCode = $"system.unhandled_exception.{context.TraceIdentifier}"
                .Replace(':', '.')
                .Replace('/', '.');

            await repository.AddSystemAlertAsync(
                new SystemAlert
                {
                    AlertCode = alertCode,
                    TriggerCode = "escalation.alert",
                    AlertType = "UnhandledException",
                    RelatedEntityName = "HttpRequest",
                    RelatedEntityId = context.TraceIdentifier,
                    Severity = SystemAlertSeverity.Critical,
                    AlertStatus = SystemAlertStatus.Open,
                    AlertMessage = $"{context.Request.Method} {context.Request.Path}: {exception.Message}",
                    SlaDueDateUtc = currentDateTime.UtcNow.AddMinutes(5),
                    EscalationLevel = 1,
                    NotificationChain = "Admin>OperationsManager",
                    CreatedBy = "GlobalExceptionMiddleware",
                    DateCreated = currentDateTime.UtcNow,
                    IPAddress = context.Connection.RemoteIpAddress?.ToString() ?? "127.0.0.1"
                },
                context.RequestAborted);

            await unitOfWork.SaveChangesAsync(context.RequestAborted);
        }
        catch (Exception trackingException)
        {
            _logger.LogWarning(trackingException, "Unable to persist system alert for unhandled exception.");
        }
    }
}
