using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace ECommerce.API.Modules.Basket;

public static class BasketModule
{
    public static IServiceCollection AddBasketModule(this IServiceCollection services)
    {
        // İleride Basket modülüne özel servisler buraya eklenecek
        return services;
    }

    public static IEndpointRouteBuilder MapBasketEndpoints(this IEndpointRouteBuilder app)
    {
        return app;
    }
}
