using ECommerce.API.Infrastructure.Database;
using ECommerce.API.Infrastructure.Handlers;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ECommerce.API.Modules.Product.Features.BulkActions.BulkDelete;

public class BulkDeleteCommandHandler : ICommandHandler<BulkDeleteCommand, BulkDeleteResult>
{
    private readonly AppDbContext _dbContext;

    public BulkDeleteCommandHandler(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<BulkDeleteResult> HandleAsync(BulkDeleteCommand command, CancellationToken cancellationToken = default)
    {
        if (command.ProductIds == null || !command.ProductIds.Any())
            return new BulkDeleteResult(0, "Silinecek ürün seçilmedi.");

        var products = await _dbContext.Products
            .Where(p => command.ProductIds.Contains(p.Id) && !p.IsDeleted)
            .ToListAsync(cancellationToken);

        int count = 0;
        foreach (var product in products)
        {
            product.MarkAsDeleted();
            count++;
        }

        if (count > 0)
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        return new BulkDeleteResult(count, $"{count} adet ürün başarıyla silindi.");
    }
}
