using ECommerce.API.Modules.Payment.Providers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace ECommerce.API.Modules.Payment;

public static class PaymentModule
{
    public static IServiceCollection AddPaymentModule(this IServiceCollection services)
    {
        services.AddScoped<IPaymentGateway, FakePaymentGateway>();
        return services;
    }

    public static IEndpointRouteBuilder MapPaymentEndpoints(this IEndpointRouteBuilder app)
    {
        // Payment Endpoints will be mapped here
        return app;
    }
}
