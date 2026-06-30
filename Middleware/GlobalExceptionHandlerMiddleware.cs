using System.Diagnostics;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.AspNetCore.Diagnostics;

namespace Middleware;

/// <summary>
/// Global exception handler that implements <see cref="IExceptionHandler"/> for centralized error handling.
/// Integrates with Application Insights for telemetry and OpenTelemetry for distributed tracing.
/// </summary>
public class GlobalExceptionHandler(
    ILogger<GlobalExceptionHandler> logger,
    TelemetryClient telemetryClient,
    IWebHostEnvironment environment) : IExceptionHandler
{
    private static readonly ActivitySource ActivitySource = new("GlobalExceptionHandler");

    /// <summary>
    /// Handles exceptions by logging, tracking telemetry, and returning a standardized error response.
    /// </summary>
    public async ValueTask<bool> TryHandleAsync(
        HttpContext context,
        Exception exception,
        CancellationToken cancellationToken)
    {
        using var activity = ActivitySource.StartActivity("GlobalException");
        
        SetActivityTags(activity, context, exception);
        
        LogException(context, exception);
        TrackTelemetry(context, exception);

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = MapExceptionToStatusCode(exception);

        var errorResponse = BuildErrorResponse(exception);
        
        await context.Response.WriteAsJsonAsync(errorResponse, cancellationToken);
        return true;
    }

    /// <summary>
    /// Sets diagnostic tags on the current activity for distributed tracing.
    /// </summary>
    private static void SetActivityTags(Activity? activity, HttpContext context, Exception exception)
    {
        if (activity is null)
            return;

        activity.SetTag("exception.type", exception.GetType().Name);
        activity.SetTag("exception.message", exception.Message);
        activity.SetTag("exception.stacktrace", exception.StackTrace);
        activity.SetTag("http.request.path", context.Request.Path.Value);
        activity.SetTag("http.request.method", context.Request.Method);
        activity.SetStatus(ActivityStatusCode.Error, exception.Message);
    }

    /// <summary>
    /// Logs the exception with relevant context information.
    /// </summary>
    private void LogException(HttpContext context, Exception exception)
    {
        logger.LogError(
            exception,
            "Unhandled exception occurred. Path: {Path}, Method: {Method}",
            context.Request.Path.Value,
            context.Request.Method);
    }

    /// <summary>
    /// Tracks exception telemetry to Azure Application Insights.
    /// </summary>
    private void TrackTelemetry(HttpContext context, Exception exception)
    {
        var exceptionTelemetry = new ExceptionTelemetry(exception)
        {
            SeverityLevel = SeverityLevel.Error
        };

        exceptionTelemetry.Properties["path"] = context.Request.Path.Value ?? string.Empty;
        exceptionTelemetry.Properties["method"] = context.Request.Method;
        exceptionTelemetry.Properties["user_agent"] = context.Request.Headers.UserAgent.ToString();
        exceptionTelemetry.Properties["timestamp"] = DateTime.UtcNow.ToString("O");

        telemetryClient.TrackException(exceptionTelemetry);
    }

    /// <summary>
    /// Builds a standardized error response, including detailed information in Development environment.
    /// </summary>
    private object BuildErrorResponse(Exception exception)
    {
        return environment.IsDevelopment()
            ? new { error = new { message = exception.Message, exceptionType = exception.GetType().Name, stackTrace = exception.StackTrace, timestamp = DateTime.UtcNow } }
            : new { error = new { message = "An error occurred while processing your request.", exceptionType = exception.GetType().Name, timestamp = DateTime.UtcNow } };
    }

    /// <summary>
    /// Maps exception types to appropriate HTTP status codes.
    /// </summary>
    private static int MapExceptionToStatusCode(Exception exception) =>
        exception switch
        {
            ArgumentNullException or ArgumentException => StatusCodes.Status400BadRequest,
            InvalidOperationException => StatusCodes.Status409Conflict,
            NotImplementedException => StatusCodes.Status501NotImplemented,
            _ => StatusCodes.Status500InternalServerError
        };
}
