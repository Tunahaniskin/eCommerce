using System;
using System.Diagnostics;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using ECommerce.API.Infrastructure.Handlers;

namespace ECommerce.API.Infrastructure.Logging;

public class LoggingCommandHandlerDecorator<TCommand, TResult> : ICommandHandler<TCommand, TResult>
{
    private readonly ICommandHandler<TCommand, TResult> _innerHandler;

    public LoggingCommandHandlerDecorator(ICommandHandler<TCommand, TResult> innerHandler)
    {
        _innerHandler = innerHandler;
    }

    public async Task<TResult> HandleAsync(TCommand command, CancellationToken cancellationToken = default)
    {
        var commandType = typeof(TCommand);
        var commandName = commandType.Name;

        // Eger uzerinde [DoNotLog] varsa, dogrudan inner handler'a git
        if (commandType.GetCustomAttribute<DoNotLogAttribute>() != null)
        {
            return await _innerHandler.HandleAsync(command, cancellationToken);
        }

        var logAttr = commandType.GetCustomAttribute<LoggableAttribute>();
        var perfAttr = commandType.GetCustomAttribute<PerformanceThresholdAttribute>();
        
        string desc = logAttr != null && !string.IsNullOrEmpty(logAttr.Description) 
            ? $" ({logAttr.Description})" 
            : string.Empty;

        // Baslangic logu
        AppLogger.Info($"[START] {commandName}{desc} başlatıldı.", command, source: commandName);

        var stopwatch = Stopwatch.StartNew();
        try
        {
            var result = await _innerHandler.HandleAsync(command, cancellationToken);
            stopwatch.Stop();

            long elapsed = stopwatch.ElapsedMilliseconds;
            int maxMs = perfAttr?.MaxMilliseconds ?? 500; // Varsayilan 500ms

            if (elapsed > maxMs)
            {
                AppLogger.Warn($"[PERF SLOW] {commandName} tamamlandı ama yavaş çalıştı! Süre: {elapsed}ms (Eşik: {maxMs}ms)", null, source: commandName, elapsedMs: elapsed);
            }
            else
            {
                AppLogger.Info($"[SUCCESS] {commandName} başarıyla tamamlandı.", null, source: commandName, elapsedMs: elapsed);
            }

            return result;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            AppLogger.Error($"[FAILED] {commandName} işleminde hata oluştu!", ex, null, source: commandName, elapsedMs: stopwatch.ElapsedMilliseconds);
            throw;
        }
    }
}
