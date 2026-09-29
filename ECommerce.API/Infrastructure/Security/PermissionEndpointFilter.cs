using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using ECommerce.API.Infrastructure.Logging;
using System.Security.Claims;
using ECommerce.API.Modules.Auth.Services;

namespace ECommerce.API.Infrastructure.Security;

public class PermissionEndpointFilter : IEndpointFilter
{
    private readonly string _requiredPermission;

    public PermissionEndpointFilter(string requiredPermission)
    {
        _requiredPermission = requiredPermission;
    }

    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var user = context.HttpContext.User;
        var userIdStr = user.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(userIdStr) || !Guid.TryParse(userIdStr, out var userId))
        {
            return Results.Unauthorized();
        }

        var permissionService = context.HttpContext.RequestServices.GetRequiredService<UserPermissionService>();
        var authSnapshot = await permissionService.GetUserAuthSnapshotAsync(userId);

        if (authSnapshot == null || authSnapshot.IsDeleted)
        {
            return Results.Unauthorized();
        }

        if (!authSnapshot.HasPermission(_requiredPermission))
        {
            AppLogger.Warn($"Güvenlik İhlali Denemesi: UserId={userId}, Eksik Yetki='{_requiredPermission}', Path={context.HttpContext.Request.Path}");

            return Results.Problem(
                statusCode: StatusCodes.Status403Forbidden,
                title: "Erişim Reddedildi",
                detail: $"Bu işlem için gerekli yetkiye ('{_requiredPermission}') sahip değilsiniz.");
        }

        return await next(context);
    }
}