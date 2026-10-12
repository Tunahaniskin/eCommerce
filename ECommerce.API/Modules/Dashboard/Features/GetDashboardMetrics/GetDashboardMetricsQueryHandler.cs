using ECommerce.API.Infrastructure.Database;
using ECommerce.API.Infrastructure.Handlers;
using ECommerce.API.Modules.Order.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ECommerce.API.Modules.Dashboard.Features.GetDashboardMetrics;

public class GetDashboardMetricsQueryHandler : IQueryHandler<GetDashboardMetricsQuery, DashboardMetricsDto>
{
    private readonly AppDbContext _dbContext;

    public GetDashboardMetricsQueryHandler(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<DashboardMetricsDto> HandleAsync(GetDashboardMetricsQuery query, CancellationToken cancellationToken = default)
    {
        var todayUtc = DateTime.UtcNow.Date;

        // 1. Ciro Bilgileri (Sadece Paid olanlar)
        var validStatuses = new[] { OrderStatus.Paid };
        
        var totalRevenue = await _dbContext.Orders
            .Where(o => validStatuses.Contains(o.Status))
            .SumAsync(o => (decimal?)o.TotalAmount, cancellationToken) ?? 0m;

        var todayRevenue = await _dbContext.Orders
            .Where(o => validStatuses.Contains(o.Status) && o.CreatedAt >= todayUtc)
            .SumAsync(o => (decimal?)o.TotalAmount, cancellationToken) ?? 0m;

        // 2. Kullanıcı Bilgileri
        var totalUsersCount = await _dbContext.Users.CountAsync(cancellationToken);
        var newUsersTodayCount = await _dbContext.Users.CountAsync(u => u.CreatedAt >= todayUtc, cancellationToken);

        // 3. Sipariş Bilgileri
        var totalOrdersCount = await _dbContext.Orders.CountAsync(cancellationToken);
        var todayOrdersCount = await _dbContext.Orders.CountAsync(o => o.CreatedAt >= todayUtc, cancellationToken);

        // 4. Ürün & Stok Bilgileri
        var activeProducts = _dbContext.Products;
        var totalProductsCount = await activeProducts.CountAsync(cancellationToken);
        var totalStockCount = await activeProducts.SumAsync(p => (int?)p.Stock, cancellationToken) ?? 0;

        // 5. Kritik Stok (Eşik = 30)
        const int threshold = 30;
        var lowStockProducts = await activeProducts
            .Where(p => p.Stock <= threshold)
            .OrderBy(p => p.Stock)
            .Take(10)
            .Select(p => new CriticalStockItemDto(p.Id, p.Name, p.Stock, p.Price))
            .ToListAsync(cancellationToken);

        var lowStockProductsCount = await activeProducts.CountAsync(p => p.Stock <= threshold, cancellationToken);

        // 6. Son Hareketler (Son 5 Sipariş)
        var recentOrdersRaw = await _dbContext.Orders
            .OrderByDescending(o => o.CreatedAt)
            .Take(5)
            .Join(_dbContext.Users.IgnoreQueryFilters(), 
                  o => o.UserId, 
                  u => u.Id, 
                  (o, u) => new { Order = o, User = u })
            .ToListAsync(cancellationToken);

        var recentOrders = recentOrdersRaw.Select(x => new RecentOrderDto(
            x.Order.Id,
            x.Order.Id.ToString().Substring(0, 8).ToUpper(),
            x.User?.Email ?? "Bilinmiyor",
            x.User != null ? $"{x.User.FirstName} {x.User.LastName}".Trim() : "Bilinmiyor",
            x.Order.TotalAmount,
            x.Order.Status.ToString(),
            x.Order.CreatedAt
        )).ToList();

        return new DashboardMetricsDto
        {
            TotalRevenue = totalRevenue,
            TodayRevenue = todayRevenue,
            TotalUsersCount = totalUsersCount,
            NewUsersTodayCount = newUsersTodayCount,
            TotalOrdersCount = totalOrdersCount,
            TodayOrdersCount = todayOrdersCount,
            TotalProductsCount = totalProductsCount,
            TotalStockCount = totalStockCount,
            LowStockThreshold = threshold,
            LowStockProductsCount = lowStockProductsCount,
            CriticalStockProducts = lowStockProducts,
            RecentOrders = recentOrders
        };
    }
}
