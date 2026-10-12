using System;
using System.Collections.Generic;

namespace ECommerce.API.Modules.Dashboard.Features.GetDashboardMetrics;

public record DashboardMetricsDto
{
    // 1. Ciro Bilgileri
    public decimal TotalRevenue { get; init; }
    public decimal TodayRevenue { get; init; }

    // 2. Kullanıcı Bilgileri
    public int TotalUsersCount { get; init; }
    public int NewUsersTodayCount { get; init; }

    // 3. Sipariş Bilgileri
    public int TotalOrdersCount { get; init; }
    public int TodayOrdersCount { get; init; }

    // 4. Ürün & Stok Bilgileri
    public int TotalProductsCount { get; init; }
    public int TotalStockCount { get; init; }

    // 5. Kritik Stok Bilgileri (Eşik: 30)
    public int LowStockThreshold { get; init; } = 30;
    public int LowStockProductsCount { get; init; }
    public List<CriticalStockItemDto> CriticalStockProducts { get; init; } = new();

    // 6. Son Hareketler (Son 5 Sipariş)
    public List<RecentOrderDto> RecentOrders { get; init; } = new();
}

public record CriticalStockItemDto(
    Guid Id,
    string Name,
    int Stock,
    decimal Price
);

public record RecentOrderDto(
    Guid Id,
    string OrderNumber,
    string UserEmail,
    string CustomerName,
    decimal TotalAmount,
    string Status,
    DateTime CreatedAt
);
