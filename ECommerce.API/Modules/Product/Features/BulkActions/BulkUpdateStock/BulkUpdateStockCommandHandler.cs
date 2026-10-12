using ECommerce.API.Infrastructure.Database;
using ECommerce.API.Infrastructure.Handlers;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ECommerce.API.Modules.Product.Features.BulkActions.BulkUpdateStock;

public class BulkUpdateStockCommandHandler : ICommandHandler<BulkUpdateStockCommand, BulkUpdateStockResult>
{
    private readonly AppDbContext _dbContext;

    public BulkUpdateStockCommandHandler(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<BulkUpdateStockResult> HandleAsync(BulkUpdateStockCommand command, CancellationToken cancellationToken = default)
    {
        if (command.ProductIds == null || !command.ProductIds.Any())
            return new BulkUpdateStockResult(0, "Güncellenecek ürün seçilmedi.");

        if (command.Value < 0 || command.Value >= 1_000_000)
            throw new ArgumentException("Geçersiz stok miktarı veya oranı.");

        var products = await _dbContext.Products
            .Where(p => command.ProductIds.Contains(p.Id) && !p.IsDeleted)
            .ToListAsync(cancellationToken);

        int count = 0;
        foreach (var product in products)
        {
            int newStock = product.Stock;
            switch (command.Action)
            {
                case "increase":
                    newStock += command.Value;
                    break;
                case "decrease":
                    newStock = Math.Max(0, newStock - command.Value);
                    break;
                case "set":
                default:
                    newStock = command.Value;
                    break;
            }
            product.UpdateDetails(product.Name, product.Price, newStock, product.CategoryId);
            count++;
        }

        if (count > 0)
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        return new BulkUpdateStockResult(count, $"{count} adet ürünün stoğu güncellendi.");
    }
}
