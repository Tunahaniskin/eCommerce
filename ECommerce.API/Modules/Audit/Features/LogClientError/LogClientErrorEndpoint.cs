using ECommerce.API.Infrastructure.Endpoints;
using ECommerce.API.Infrastructure.Handlers;
using System.Security.Claims;
using System.Text.Json;

namespace ECommerce.API.Modules.Audit.Features.LogClientError;

public class LogClientErrorEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        // AllowAnonymous olacak şekilde /logs/client endpoint'ini açıyoruz.
        app.MapPost("/logs/client", async (
            LogClientErrorRequest request, 
            HttpContext httpContext, 
            ICommandHandler<LogClientErrorCommand, LogClientErrorResult> handler) =>
        {
            // Kullanıcı IP'si
            var clientIp = httpContext.Connection.RemoteIpAddress?.ToString();
            
            // Tarayıcı bilgisi
            var userAgent = httpContext.Request.Headers.UserAgent.ToString();
            
            // Kullanıcı id'si (eğer token varsa)
            var userId = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                      ?? httpContext.User.FindFirst("sub")?.Value;

            var command = new LogClientErrorCommand(
                Level: request.Level ?? "Error",
                Message: request.Message ?? "Unknown Error",
                Url: request.Url ?? string.Empty,
                StackTrace: request.StackTrace,
                Metadata: request.Metadata,
                UserAgent: userAgent,
                ClientIp: clientIp,
                UserId: userId
            );

            var result = await handler.HandleAsync(command);

            return Results.Ok(new { Message = "Log kaydedildi.", LogId = result.LogId });
        })
        .AllowAnonymous();
    }
}

public record LogClientErrorRequest(
    string? Level,
    string? Message,
    string? Url,
    string? StackTrace,
    JsonElement? Metadata
);
