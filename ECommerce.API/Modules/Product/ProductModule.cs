using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace ECommerce.API.Modules.Product;

public static class ProductModule
{
    public static IServiceCollection AddProductModule(this IServiceCollection services)
    {
        return services;
    }

    public static IEndpointRouteBuilder MapProductEndpoints(this IEndpointRouteBuilder app)
    {
        return app;
    }
}
