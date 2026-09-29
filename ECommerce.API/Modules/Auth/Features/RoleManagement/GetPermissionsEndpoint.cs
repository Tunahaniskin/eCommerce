using System.Reflection;
using ECommerce.API.Infrastructure.Endpoints;
using ECommerce.API.Infrastructure.Security;
using ECommerce.API.Modules.Auth.Constants;

namespace ECommerce.API.Modules.Auth.Features.RoleManagement;

public class GetPermissionsEndpoint : IAdminEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/permissions", () =>
        {
            var permissions = new List<string>();
            
            // 1. Ana Permissions sınıfındaki const'ları ekle (örneğin "*" Wildcard)
            var mainFields = typeof(Permissions).GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
                .Where(fi => fi.IsLiteral && !fi.IsInitOnly && fi.FieldType == typeof(string));
                
            foreach (var field in mainFields)
            {
                var value = field.GetRawConstantValue() as string;
                if (!string.IsNullOrEmpty(value))
                {
                    permissions.Add(value);
                }
            }
            
            // 2. Permissions sınıfı içindeki tüm iç içe sınıfları (Nested Types - Product, Order vs.) bul
            var nestedTypes = typeof(Permissions).GetNestedTypes(BindingFlags.Public | BindingFlags.Static);
            
            foreach (var type in nestedTypes)
            {
                // Her bir iç sınıf içindeki constant (const) string alanları al
                var fields = type.GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
                    .Where(fi => fi.IsLiteral && !fi.IsInitOnly && fi.FieldType == typeof(string));

                foreach (var field in fields)
                {
                    var value = field.GetRawConstantValue() as string;
                    if (!string.IsNullOrEmpty(value))
                    {
                        permissions.Add(value);
                    }
                }
            }

            return Results.Ok(permissions);
        })
        .AddEndpointFilter(new PermissionEndpointFilter(Permissions.Roles.View));
    }
}
