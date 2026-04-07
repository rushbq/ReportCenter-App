using Microsoft.Data.SqlClient;

namespace ReportCenter.Web.Middleware;

/// <summary>
/// 記錄未處理例外的中介層，保留 request 與 SQL 例外診斷資訊。
/// </summary>
public class UnhandledExceptionLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<UnhandledExceptionLoggingMiddleware> _logger;

    public UnhandledExceptionLoggingMiddleware(
        RequestDelegate next,
        ILogger<UnhandledExceptionLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            LogUnhandledException(context, ex);
            throw;
        }
    }

    private void LogUnhandledException(HttpContext context, Exception exception)
    {
        var traceId = context.TraceIdentifier;
        var requestPath = context.Request.Path.Value ?? string.Empty;
        var method = context.Request.Method;
        var userName = context.User.Identity?.Name ?? "Anonymous";

        if (exception is SqlException sqlException)
        {
            _logger.LogError(
                sqlException,
                "未處理 SQL 例外。TraceId={TraceId} RequestPath={RequestPath} Method={Method} UserName={UserName} SqlNumber={SqlNumber} Procedure={Procedure} LineNumber={LineNumber} State={State}",
                traceId,
                requestPath,
                method,
                userName,
                sqlException.Number,
                sqlException.Procedure,
                sqlException.LineNumber,
                sqlException.State);
            return;
        }

        _logger.LogError(
            exception,
            "未處理例外。TraceId={TraceId} RequestPath={RequestPath} Method={Method} UserName={UserName}",
            traceId,
            requestPath,
            method,
            userName);
    }
}
