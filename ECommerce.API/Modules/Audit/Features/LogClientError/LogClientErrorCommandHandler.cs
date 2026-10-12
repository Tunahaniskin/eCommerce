using ECommerce.API.Infrastructure.Database;
using ECommerce.API.Infrastructure.Database.Entities;
using ECommerce.API.Infrastructure.Handlers;
using System.Text.Json;

namespace ECommerce.API.Modules.Audit.Features.LogClientError;

public class LogClientErrorCommandHandler : ICommandHandler<LogClientErrorCommand, LogClientErrorResult>
{
    private readonly AppDbContext _dbContext;

    public LogClientErrorCommandHandler(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<LogClientErrorResult> HandleAsync(LogClientErrorCommand request, CancellationToken cancellationToken = default)
    {
        // 1. Ek bilgileri birleştirip JSON string'e çevirelim
        var payloadObj = new
        {
            request.Url,
            request.StackTrace,
            Metadata = request.Metadata,
            request.UserAgent,
            request.ClientIp
        };

        string payloadStr = JsonSerializer.Serialize(payloadObj);

        // 2. Application Log olarak yeni SystemLog örneği oluştur
        var log = SystemLog.CreateAppLog(
            traceId: null, // İsteğe bağlı
            userId: request.UserId,
            logType: "Frontend",
            level: string.IsNullOrWhiteSpace(request.Level) ? "Error" : request.Level,
            source: "ClientApp",
            message: request.Message,
            payload: payloadStr,
            elapsedMs: null,
            exception: request.StackTrace
        );

        // 3. Veritabanına kaydet
        _dbContext.SystemLogs.Add(log);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return new LogClientErrorResult(true, log.Id);
    }
}
