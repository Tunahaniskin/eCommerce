using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using ECommerce.API.Modules.Catalog.Features.CreateProduct;
using ECommerce.API.Modules.Catalog.Features.GetProducts;
using ECommerce.API.Modules.Catalog.Features.DeleteProduct;

namespace ECommerce.API.Modules.Catalog;

public static class CatalogModule
{
    public static IServiceCollection AddCatalogModule(this IServiceCollection services)
    {
        return services;
    }

    public static IEndpointRouteBuilder MapCatalogEndpoints(this IEndpointRouteBuilder app)
    {
        
        return app;
    }
}