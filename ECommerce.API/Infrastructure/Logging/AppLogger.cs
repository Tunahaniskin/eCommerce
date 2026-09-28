using System;
using System.Security.Claims;
using System.Text.Json;
using ECommerce.API.Infrastructure.Database.Entities;
using Microsoft.AspNetCore.Http;

namespace ECommerce.API.Infrastructure.Logging;

public enum AppLogLevel
{
    Debug,
    Info,
    Warning,
    Error,
    Critical
}

public static class AppLogger
{
    private static readonly object _lock = new();
    private static IHttpContextAccessor? _httpContextAccessor;

    public static void Configure(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public static void Info(string message, object? data = null, string? source = null, long? elapsedMs = null) => 
        Log(AppLogLevel.Info, message, data, null, ConsoleColor.Cyan, source, elapsedMs);

    public static void Warn(string message, object? data = null, string? source = null, long? elapsedMs = null) => 
        Log(AppLogLevel.Warning, message, data, null, ConsoleColor.Yellow, source, elapsedMs);

    public static void Error(string message, Exception? ex = null, object? data = null, string? source = null, long? elapsedMs = null) => 
        Log(AppLogLevel.Error, message, data, ex, ConsoleColor.Red, source, elapsedMs);

    public static void Debug(string message, object? data = null, string? source = null, long? elapsedMs = null) => 
        Log(AppLogLevel.Debug, message, data, null, ConsoleColor.DarkGray, source, elapsedMs);

    private static void Log(AppLogLevel level, string message, object? data, Exception? ex, ConsoleColor color, string? source, long? elapsedMs)
    {
        var traceId = _httpContextAccessor?.HttpContext?.TraceIdentifier;
        var userId = _httpContextAccessor?.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier) ?? "System";
        string? payload = data != null ? JsonSerializer.Serialize(data) : null;
        string? exceptionStr = ex != null ? $"{ex.Message} | StackTrace: {ex.StackTrace}" : null;

        // 1. Veritabanına (Arka Plan Kuyruğuna) Yaz
        var systemLog = SystemLog.CreateAppLog(
            traceId: traceId,
            userId: userId,
            logType: "Application",
            level: level.ToString(),
            source: source,
            message: message,
            payload: payload,
            elapsedMs: elapsedMs,
            exception: exceptionStr
        );

        LogQueue.Channel.Writer.TryWrite(systemLog);

        // 2. Konsola Yaz (Hala renkli çıktı görmek için)
        lock (_lock)
        {
            var originalColor = Console.ForegroundColor;
            Console.ForegroundColor = color;

            string time = DateTime.UtcNow.AddHours(3).ToString("HH:mm:ss.fff");
            string userStr = userId != "System" ? $"[User:{userId}] " : "";
            string traceStr = traceId != null ? $"[Trace:{traceId}] " : "";
            string logLine = $"[{level.ToString().ToUpper()}] [{time}] {userStr}{traceStr}{message}";
            
            if (payload != null)
            {
                logLine += $" | Data: {payload}";
            }

            Console.WriteLine(logLine);

            if (ex != null)
            {
                Console.WriteLine($"    [EXCEPTION] {ex.Message}");
                Console.WriteLine($"    [STACKTRACE] {ex.StackTrace}");
            }

            Console.ForegroundColor = originalColor;
        }
    }
}
