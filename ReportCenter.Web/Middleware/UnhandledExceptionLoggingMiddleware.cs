using System.Diagnostics;
using Microsoft.Data.SqlClient;
using Serilog.Context;

namespace ReportCenter.Web.Middleware;

/// <summary>
/// 記錄未處理例外的中介層。
/// 以結構化屬性記錄完整 request context 與 SQL 診斷資訊，
/// 方便 Seq / File Sink 查詢與告警。
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
            LogException(context, ex);
            throw;  // 重新拋出，交由 ExceptionHandler / DeveloperExceptionPage 處理
        }
    }

    private void LogException(HttpContext ctx, Exception exception)
    {
        // 共用結構化屬性 — 推入 LogContext，File / Seq 都能索引
        using (LogContext.PushProperty("TraceId", ctx.TraceIdentifier))
        using (LogContext.PushProperty("RequestPath", ctx.Request.Path.Value ?? ""))
        using (LogContext.PushProperty("HttpMethod", ctx.Request.Method))
        using (LogContext.PushProperty("UserName", ctx.User.Identity?.Name ?? "Anonymous"))
        using (LogContext.PushProperty("QueryString", ctx.Request.QueryString.Value ?? ""))
        using (LogContext.PushProperty("ClientIp", ctx.Connection.RemoteIpAddress?.ToString() ?? "-"))
        {
            if (exception is SqlException sqlEx)
            {
                _logger.LogError(sqlEx,
                    "未處理 SQL 例外 — Number={SqlNumber} Procedure={Procedure} Line={SqlLine} State={SqlState}",
                    sqlEx.Number, sqlEx.Procedure ?? "-", sqlEx.LineNumber, sqlEx.State);
            }
            else
            {
                _logger.LogError(exception, "未處理例外 — {ExceptionType}", exception.GetType().Name);
            }
        }
    }
}
