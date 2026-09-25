using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace ECommerce.API.Modules.Shipment;

public static class ShipmentModule
{
    public static IServiceCollection AddShipmentModule(this IServiceCollection services)
    {
        // İleride Shipment modülüne özel servisler buraya eklenecek
        return services;
    }

    public static IEndpointRouteBuilder MapShipmentEndpoints(this IEndpointRouteBuilder app)
    {
        return app;
    }
}
