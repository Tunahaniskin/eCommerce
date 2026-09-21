using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace ECommerce.API.Infrastructure.Endpoints;

public static class EndpointExtensions
{
    public static IEndpointRouteBuilder MapEndpoints(this IEndpointRouteBuilder app, Assembly assembly)
    {
        // 1. Grupları ve güvenlik sınırlarını tanımla
        var publicGroup = app.MapGroup("/api");
        var adminGroup = app.MapGroup("/api/admin")
            .RequireAuthorization(policy => policy.RequireRole("Admin"));

        // 2. Assembly içindeki tüm endpoint sınıflarını tara
        var endpointTypes = assembly.GetTypes()
            .Where(t => typeof(IEndpoint).IsAssignableFrom(t) && 
                        !t.IsInterface && 
                        !t.IsAbstract);

        // 3. Tipine göre ilgili gruba bağla
        foreach (var type in endpointTypes)
        {
            if (Activator.CreateInstance(type) is IEndpoint endpoint)
            {
                if (endpoint is IAdminEndpoint)
                {
                    endpoint.MapEndpoint(adminGroup);
                }
                else
                {
                    endpoint.MapEndpoint(publicGroup);
                }
            }
        }

        return app;
    }
}