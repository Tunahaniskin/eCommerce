using System.Security.Claims;
using ECommerce.API.Modules.Auth.Constants;

namespace ECommerce.API.Infrastructure.Extensions;

public static class ClaimsPrincipalExtensions
{
    public static bool HasPermission(this ClaimsPrincipal user, string requiredPermission)
    {
        if (user.Identity?.IsAuthenticated != true)
        {
            return false;
        }

        // 1. SuperAdmin rolü doğuştan tüm yetkilere sahiptir.
        if (user.IsInRole("SuperAdmin"))
        {
            return true;
        }

        // Token içerisindeki "permission" claim'lerini topla
        var permissions = user.FindAll("permission")
                              .Select(c => c.Value)
                              .ToHashSet();

        // 2. Wildcard (*) joker yetkisi var mı?
        if (permissions.Contains(Permissions.Wildcard))
        {
            return true;
        }

        // 3. İstenen spesifik yetki mevcut mu?
        return permissions.Contains(requiredPermission);
    }
}