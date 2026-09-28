using ECommerce.API.Infrastructure.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using ECommerce.API.Infrastructure.Logging;
using System.Security.Claims;

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

        if (!user.HasPermission(_requiredPermission))
        {
            var userId = user.FindFirstValue(ClaimTypes.NameIdentifier) ?? "Anonim";
            
            AppLogger.Warn($"Güvenlik İhlali Denemesi: UserId={userId}, Eksik Yetki='{_requiredPermission}', Path={context.HttpContext.Request.Path}");

            return Results.Problem(
                statusCode: StatusCodes.Status403Forbidden,
                title: "Erişim Reddedildi",
                detail: $"Bu işlem için gerekli yetkiye ('{_requiredPermission}') sahip değilsiniz.");
        }

        return await next(context);
    }
}