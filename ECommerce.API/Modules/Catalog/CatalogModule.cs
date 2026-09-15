using ECommerce.API.Modules.Catalog.Features.CreateProduct;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace ECommerce.API.Modules.Catalog;

public static class CatalogModule
{
    public static IServiceCollection AddCatalogModule(this IServiceCollection services)
    {
        return services;
    }

    public static IEndpointRouteBuilder MapCatalogEndpoints(this IEndpointRouteBuilder app)
    {
        CreateProductEndpoint.MapEndpoint(app);
        return app;
    }
}