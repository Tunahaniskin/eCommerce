using ECommerce.API.Infrastructure.Endpoints;
using ECommerce.API.Infrastructure.Extensions;
using ECommerce.API.Modules.Auth.Constants;
using ECommerce.API.Infrastructure.Handlers;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace ECommerce.API.Modules.Auth.Features.RoleManagement.AssignTemporaryPermission;

public class AssignTemporaryPermissionEndpoint : IAdminEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/users/{userId:guid}/permissions/temporary", async (Guid userId, AssignTemporaryPermissionRequest request, HttpContext httpContext, ICommandHandler<AssignTemporaryPermissionCommand, AssignTemporaryPermissionResult> handler) =>
        {
            if (string.IsNullOrWhiteSpace(request.Permission))
                return Results.BadRequest(new { Message = "Permission boş olamaz." });
            if (request.DurationInHours <= 0)
                return Results.BadRequest(new { Message = "Duration 0'dan büyük olmalıdır." });

            var adminIdClaim = httpContext.User.FindFirst(ClaimTypes.NameIdentifier);
            if (adminIdClaim == null || !Guid.TryParse(adminIdClaim.Value, out Guid adminId))
                return Results.Unauthorized();

            var command = new AssignTemporaryPermissionCommand(userId, request.Permission, request.DurationInHours, adminId);
            var result = await handler.HandleAsync(command);

            return Results.Ok(new { Message = result.Message });
        })
        .RequirePermission(Permissions.Roles.Manage);
    }
}

public record AssignTemporaryPermissionRequest(string Permission, int DurationInHours);
