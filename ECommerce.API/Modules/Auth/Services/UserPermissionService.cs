using ECommerce.API.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using ECommerce.API.Modules.Auth.Constants;

namespace ECommerce.API.Modules.Auth.Services;

public class UserAuthSnapshot
{
    public Guid UserId { get; set; }
    public Guid RoleId { get; set; }
    public bool IsDeleted { get; set; }
    public HashSet<string> Permissions { get; set; } = new();

    public bool HasPermission(string requiredPermission)
    {
        if (IsDeleted) return false;
        
        // SuperAdmin / Wildcard
        if (Permissions.Contains(Constants.Permissions.Wildcard)) return true;
        
        // Exact match
        return Permissions.Contains(requiredPermission);
    }
}

public class UserPermissionService
{
    private readonly AppDbContext _dbContext;
    private readonly IMemoryCache _memoryCache;

    public UserPermissionService(AppDbContext dbContext, IMemoryCache memoryCache)
    {
        _dbContext = dbContext;
        _memoryCache = memoryCache;
    }

    public async Task<UserAuthSnapshot?> GetUserAuthSnapshotAsync(Guid userId)
    {
        string cacheKey = $"user_auth_{userId}";

        return await _memoryCache.GetOrCreateAsync(cacheKey, async entry =>
        {
            var user = await _dbContext.Users
                .AsNoTracking()
                .Include(u => u.Role)
                .ThenInclude(r => r.Permissions)
                .Include(u => u.TemporaryPermissions)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(1);
                return null;
            }

            var permissions = new HashSet<string>();

            // 1. Rolün sabit yetkileri
            if (user.Role?.Permissions != null)
            {
                foreach (var rp in user.Role.Permissions)
                {
                    permissions.Add(rp.Permission);
                }
            }

            // 2. Süresi dolmamış geçici yetkiler
            var now = DateTime.UtcNow;
            DateTime? earliestExpiration = null;

            if (user.TemporaryPermissions != null)
            {
                foreach (var tp in user.TemporaryPermissions)
                {
                    if (tp.ExpiresAt == null || tp.ExpiresAt > now)
                    {
                        permissions.Add(tp.Permission);
                        
                        if (tp.ExpiresAt.HasValue)
                        {
                            if (earliestExpiration == null || tp.ExpiresAt < earliestExpiration)
                            {
                                earliestExpiration = tp.ExpiresAt;
                            }
                        }
                    }
                }
            }

            var snapshot = new UserAuthSnapshot
            {
                UserId = user.Id,
                RoleId = user.RoleId,
                IsDeleted = user.IsDeleted,
                Permissions = permissions
            };

            // Cache süresi: En yakın bitiş süreli yetkiye göre veya max 5 dakika
            var maxCacheDuration = TimeSpan.FromMinutes(5);
            if (earliestExpiration.HasValue)
            {
                var timeUntilExpiration = earliestExpiration.Value - now;
                if (timeUntilExpiration > TimeSpan.Zero && timeUntilExpiration < maxCacheDuration)
                {
                    entry.AbsoluteExpirationRelativeToNow = timeUntilExpiration;
                }
                else
                {
                    entry.AbsoluteExpirationRelativeToNow = maxCacheDuration;
                }
            }
            else
            {
                entry.AbsoluteExpirationRelativeToNow = maxCacheDuration;
            }

            return snapshot;
        });
    }

    public void InvalidateUserCache(Guid userId)
    {
        _memoryCache.Remove($"user_auth_{userId}");
    }
}
