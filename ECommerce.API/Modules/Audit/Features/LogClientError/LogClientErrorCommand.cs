using ECommerce.API.Infrastructure.Handlers;
using System.Text.Json;

namespace ECommerce.API.Modules.Audit.Features.LogClientError;

public record LogClientErrorCommand(
    string Level,
    string Message,
    string Url,
    string? StackTrace,
    JsonElement? Metadata,
    string? UserAgent,
    string? ClientIp,
    string? UserId
);

public record LogClientErrorResult(bool Success, Guid LogId);
