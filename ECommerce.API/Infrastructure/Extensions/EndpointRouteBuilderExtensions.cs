using ECommerce.API.Infrastructure.Security;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace ECommerce.API.Infrastructure.Extensions;

public static class EndpointRouteBuilderExtensions
{
    /// <summary>
    /// Endpoint'i belirtilen yetki (permission) ile korur.
    /// Giriş yapmayanlara 401, yetkisi olmayanlara 403 döner.
    /// </summary>
    public static RouteHandlerBuilder RequirePermission(this RouteHandlerBuilder builder, string permission)
    {
        return builder
            .RequireAuthorization() 
            .AddEndpointFilter(new PermissionEndpointFilter(permission));
    }
}