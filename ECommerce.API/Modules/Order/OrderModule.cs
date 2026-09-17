using ECommerce.API.Modules.Order.Features.CreateOrder;
using Microsoft.Extensions.DependencyInjection;

namespace ECommerce.API.Modules.Order;

public static class OrderModule
{
    public static IServiceCollection AddOrderModule(this IServiceCollection services)
    {
        // İleride Order modülüne özel servisler buraya eklenecek
        return services;
    }

    public static IEndpointRouteBuilder MapOrderEndpoints(this IEndpointRouteBuilder app)
    {
        
        return app;
    }
}