using ECommerce.API.Infrastructure.Database;
using ECommerce.API.Infrastructure.Handlers;
using ECommerce.API.Modules.Order.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ECommerce.API.Modules.Auth.Features.RoleManagement.GetCustomer360;

public class GetCustomer360QueryHandler : IQueryHandler<GetCustomer360Query, Customer360ResponseDto?>
{
    private readonly AppDbContext _dbContext;

    public GetCustomer360QueryHandler(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Customer360ResponseDto?> HandleAsync(GetCustomer360Query query, CancellationToken cancellationToken = default)
    {
        var user = await _dbContext.Users
            .Include(u => u.Role)
            .Include(u => u.Addresses)
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(u => u.Id == query.UserId, cancellationToken);

        if (user == null) return null;

        var profile = new Customer360ProfileDto(
            user.Id,
            $"{user.FirstName} {user.LastName}".Trim(),
            user.Email,
            user.CreatedAt,
            user.IsDeleted,
            user.Role?.Name ?? "Bilinmiyor"
        );

        var addresses = user.Addresses.Select(a => new Customer360AddressDto(
            a.Id,
            a.Title,
            a.RecipientName,
            a.PhoneNumber,
            a.City,
            a.District,
            a.FullAddress
        )).ToList();

        var orders = await _dbContext.Orders
            .Include(o => o.Items)
            .Where(o => o.UserId == query.UserId)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync(cancellationToken);

        var validStatuses = new[] { OrderStatus.Paid };

        var totalSpent = orders
            .Where(o => validStatuses.Contains(o.Status))
            .Sum(o => o.TotalAmount);
        
        var totalOrdersCount = orders.Count;
        var refundedOrdersCount = orders.Count(o => o.Status == OrderStatus.Refunded);

        var recentOrders = orders.Take(5).Select(o => new Customer360RecentOrderDto(
            o.Id,
            o.CreatedAt,
            o.TotalAmount,
            o.Items.Sum(i => i.Quantity),
            o.Status.ToString()
        )).ToList();

        var wallet = await _dbContext.Wallets
            .FirstOrDefaultAsync(w => w.UserId == query.UserId, cancellationToken);
            
        var currentBalance = wallet?.Balance ?? 0m;

        var walletTransactions = new System.Collections.Generic.List<Customer360WalletTransactionDto>();
        if (wallet != null)
        {
            var transactions = await _dbContext.WalletTransactions
                .Where(wt => wt.WalletId == wallet.Id)
                .OrderByDescending(wt => wt.CreatedAt)
                .Take(10)
                .ToListAsync(cancellationToken);

            walletTransactions = transactions.Select(wt => new Customer360WalletTransactionDto(
                wt.Id,
                wt.Type.ToString(),
                wt.Amount,
                0m, // BalanceAfter is not in WalletTransaction entity currently, assuming it's not tracked directly
                wt.Description ?? "",
                wt.CreatedAt,
                null
            )).ToList();
        }

        var stats = new Customer360StatsDto(
            currentBalance,
            totalSpent,
            totalOrdersCount,
            refundedOrdersCount
        );

        return new Customer360ResponseDto(
            profile,
            stats,
            addresses,
            recentOrders,
            walletTransactions
        );
    }
}
