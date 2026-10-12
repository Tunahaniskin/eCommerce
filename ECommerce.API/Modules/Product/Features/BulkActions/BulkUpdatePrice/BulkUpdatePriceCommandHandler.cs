using ECommerce.API.Infrastructure.Database;
using ECommerce.API.Infrastructure.Handlers;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ECommerce.API.Modules.Product.Features.BulkActions.BulkUpdatePrice;

public class BulkUpdatePriceCommandHandler : ICommandHandler<BulkUpdatePriceCommand, BulkUpdatePriceResult>
{
    private readonly AppDbContext _dbContext;

    public BulkUpdatePriceCommandHandler(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<BulkUpdatePriceResult> HandleAsync(BulkUpdatePriceCommand command, CancellationToken cancellationToken = default)
    {
        if (command.ProductIds == null || !command.ProductIds.Any())
            return new BulkUpdatePriceResult(0, "Güncellenecek ürün seçilmedi.");

        if (command.Value < 0 || command.Value >= 10_000_000)
            throw new ArgumentException("Geçersiz fiyat miktarı veya oranı.");

        var products = await _dbContext.Products
            .Where(p => command.ProductIds.Contains(p.Id) && !p.IsDeleted)
            .ToListAsync(cancellationToken);

        int count = 0;
        foreach (var product in products)
        {
            decimal newPrice = product.Price;
            switch (command.Action)
            {
                case "fixed_increase":
                    newPrice += command.Value;
                    break;
                case "fixed_decrease":
                    newPrice = Math.Max(0, newPrice - command.Value);
                    break;
                case "percent_increase":
                    newPrice += newPrice * (command.Value / 100);
                    break;
                case "percent_decrease":
                    newPrice = Math.Max(0, newPrice - (newPrice * (command.Value / 100)));
                    break;
                case "set":
                default:
                    newPrice = command.Value;
                    break;
            }
            product.UpdateDetails(product.Name, newPrice, product.Stock, product.CategoryId);
            count++;
        }

        if (count > 0)
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        return new BulkUpdatePriceResult(count, $"{count} adet ürünün fiyatı güncellendi.");
    }
}
