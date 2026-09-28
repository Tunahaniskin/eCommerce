using System;
using System.Diagnostics;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using ECommerce.API.Infrastructure.Handlers;

namespace ECommerce.API.Infrastructure.Logging;

public class LoggingQueryHandlerDecorator<TQuery, TResult> : IQueryHandler<TQuery, TResult>
{
    private readonly IQueryHandler<TQuery, TResult> _innerHandler;

    public LoggingQueryHandlerDecorator(IQueryHandler<TQuery, TResult> innerHandler)
    {
        _innerHandler = innerHandler;
    }

    public async Task<TResult> HandleAsync(TQuery query, CancellationToken cancellationToken = default)
    {
        var queryType = typeof(TQuery);
        var queryName = queryType.Name;

        if (queryType.GetCustomAttribute<DoNotLogAttribute>() != null)
        {
            return await _innerHandler.HandleAsync(query, cancellationToken);
        }

        var logAttr = queryType.GetCustomAttribute<LoggableAttribute>();
        var perfAttr = queryType.GetCustomAttribute<PerformanceThresholdAttribute>();
        
        string desc = logAttr != null && !string.IsNullOrEmpty(logAttr.Description) 
            ? $" ({logAttr.Description})" 
            : string.Empty;

        AppLogger.Debug($"[START] {queryName}{desc} çalıştırılıyor...", query);

        var stopwatch = Stopwatch.StartNew();
        try
        {
            var result = await _innerHandler.HandleAsync(query, cancellationToken);
            stopwatch.Stop();

            long elapsed = stopwatch.ElapsedMilliseconds;
            int maxMs = perfAttr?.MaxMilliseconds ?? 200; // Varsayilan 200ms (Sorgular hizli olmali)

            if (elapsed > maxMs)
            {
                AppLogger.Warn($"[PERF SLOW] {queryName} tamamlandı ama yavaş çalıştı! Süre: {elapsed}ms (Eşik: {maxMs}ms)");
            }
            else
            {
                AppLogger.Debug($"[SUCCESS] {queryName} {elapsed}ms içinde tamamlandı.");
            }

            return result;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            AppLogger.Error($"[FAILED] {queryName} sorgusunda {stopwatch.ElapsedMilliseconds}ms sonra hata oluştu!", ex);
            throw;
        }
    }
}
