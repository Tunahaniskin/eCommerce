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
                // 1. Asıl handler'ı kendi sınıf adıyla kaydet (Decorator içinden çağırabilmek için)
                services.AddScoped(handler);

                // 2. Arayüz istendiğinde (Endpoint üzerinden) Decorator üretip asıl handler'ı içine ver
                services.AddScoped(commandInterface, sp =>
                {
                    var innerHandler = sp.GetRequiredService(handler);
                    var decoratorType = typeof(ECommerce.API.Infrastructure.Logging.LoggingCommandHandlerDecorator<,>)
                        .MakeGenericType(commandInterface.GetGenericArguments());
                    
                    return ActivatorUtilities.CreateInstance(sp, decoratorType, innerHandler);
                });
                
                continue; // Kaydettiysek sonrakine geç
            }

            // Query Handler kontrolü
            var queryInterface = handler.GetInterfaces()
                .FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(ECommerce.API.Infrastructure.Handlers.IQueryHandler<,>));

            if (queryInterface != null)
            {
                // 1. Asıl query handler'ı kendi sınıf adıyla kaydet
                services.AddScoped(handler);

                // 2. Arayüz istendiğinde Query Decorator üretip asıl handler'ı içine ver
                services.AddScoped(queryInterface, sp =>
                {
                    var innerHandler = sp.GetRequiredService(handler);
                    var decoratorType = typeof(ECommerce.API.Infrastructure.Logging.LoggingQueryHandlerDecorator<,>)
                        .MakeGenericType(queryInterface.GetGenericArguments());
                    
                    return ActivatorUtilities.CreateInstance(sp, decoratorType, innerHandler);
                });
            }
        }

        return services;
    }
}
