using ECommerce.API.Infrastructure.Endpoints;
using ECommerce.API.Modules.Auth.Constants;
using ECommerce.API.Infrastructure.Handlers;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.Security.Claims;

namespace ECommerce.API.Modules.Audit.Features.GetLogs;

public class GetLogsEndpoint : IAdminEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        // Admin-only (IAdminEndpoint automatically maps under /api/admin group)
        app.MapGet("/logs", async (
            [AsParameters] GetLogsRequest request,
            HttpContext httpContext,
            IQueryHandler<GetLogsQuery, GetLogsResponse> handler,
            ECommerce.API.Modules.Auth.Services.UserPermissionService permissionService) =>
        {
            var user = httpContext.User;
            var userIdStr = user.FindFirst(ClaimTypes.NameIdentifier)?.Value 
                            ?? user.FindFirst("sub")?.Value;

            if (!Guid.TryParse(userIdStr, out var userId))
            {
                return Results.Unauthorized();
            }

            var authSnapshot = await permissionService.GetUserAuthSnapshotAsync(userId);
            if (authSnapshot == null || !authSnapshot.HasPermission(Permissions.Logs.View))
            {
                return Results.Problem(
                    statusCode: StatusCodes.Status403Forbidden,
                    title: "Erişim Reddedildi",
                    detail: "Log kayıtlarını görüntüleme yetkiniz bulunmuyor.");
            }

            var query = new GetLogsQuery(
                request.Page, 
                request.PageSize, 
                request.LogType, 
                request.Level, 
                request.SearchText,
                request.Search,
                request.StartDate,
                request.EndDate
            );
            
            var logs = await handler.HandleAsync(query);

            return Results.Ok(logs);
        });
    }
}

public record GetLogsRequest(
    int Page = 1, 
    int PageSize = 20, 
    string? LogType = "all", 
    string? Level = "all", 
    string? SearchText = null,
    string? Search = null,
    DateTime? StartDate = null,
    DateTime? EndDate = null
);
