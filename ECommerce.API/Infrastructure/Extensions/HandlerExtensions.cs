using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace ECommerce.API.Infrastructure.Extensions;

public static class HandlerExtensions
{
    public static IServiceCollection AddCommandHandlers(this IServiceCollection services, Assembly assembly)
    {
        // Assembly'deki sınıfları tara: Soyut (abstract) veya Interface olmayan sınıfları bul.
        var handlers = assembly.GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract);

        foreach (var handler in handlers)
        {
            // Command Handler kontrolü
            var commandInterface = handler.GetInterfaces()
                .FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(ECommerce.API.Infrastructure.Handlers.ICommandHandler<,>));

            if (commandInterface != null)
            {
                services.AddScoped(commandInterface, handler);
                continue; // Kaydettiysek sonrakine geç
            }

            // Query Handler kontrolü
            var queryInterface = handler.GetInterfaces()
                .FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(ECommerce.API.Infrastructure.Handlers.IQueryHandler<,>));

            if (queryInterface != null)
            {
                services.AddScoped(queryInterface, handler);
            }
        }

        return services;
    }
}
