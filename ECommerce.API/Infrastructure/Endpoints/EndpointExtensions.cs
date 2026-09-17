using System.Reflection;

namespace ECommerce.API.Infrastructure.Endpoints;

public static class EndpointExtensions
{
    public static IEndpointRouteBuilder MapEndpoints(this IEndpointRouteBuilder app, Assembly assembly)
    {
        // 1. İlgili Assembly içindeki tüm sınıfları tara
        var endpointTypes = assembly.GetTypes()
            .Where(t => typeof(IEndpoint).IsAssignableFrom(t) && 
                        !t.IsInterface && 
                        !t.IsAbstract);

        // 2. Bulunan her sınıftan bir nesne örneği üret ve MapEndpoint metodunu çalıştır
        foreach (var type in endpointTypes)
        {
            if (Activator.CreateInstance(type) is IEndpoint endpoint)
            {
                endpoint.MapEndpoint(app);
            }
        }

        return app;
    }
}