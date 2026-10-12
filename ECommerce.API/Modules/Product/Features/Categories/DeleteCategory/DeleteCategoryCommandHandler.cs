using ECommerce.API.Infrastructure.Database;
using ECommerce.API.Infrastructure.Handlers;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ECommerce.API.Modules.Product.Features.Categories.DeleteCategory;

public class DeleteCategoryCommandHandler : ICommandHandler<DeleteCategoryCommand, DeleteCategoryResult>
{
    private readonly AppDbContext _dbContext;

    public DeleteCategoryCommandHandler(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<DeleteCategoryResult> HandleAsync(DeleteCategoryCommand command, CancellationToken cancellationToken = default)
    {
        var category = await _dbContext.Categories
            .Include(c => c.Products.Where(p => !p.IsDeleted))
            .FirstOrDefaultAsync(c => c.Id == command.Id && !c.IsDeleted, cancellationToken);

        if (category == null)
        {
            throw new InvalidOperationException("Kategori bulunamadı veya zaten silinmiş.");
        }

        var productCount = category.Products.Count;

        if (productCount > 0)
        {
            if (!command.TargetCategoryId.HasValue)
            {
                throw new ArgumentException($"Bu kategoride {productCount} adet ürün bulunmaktadır. Ürünlerin aktarılacağı yeni kategori seçilmelidir.");
            }

            var targetCategory = await _dbContext.Categories
                .FirstOrDefaultAsync(c => c.Id == command.TargetCategoryId.Value && !c.IsDeleted, cancellationToken);

            if (targetCategory == null)
            {
                throw new InvalidOperationException("Aktarılacak hedef kategori bulunamadı.");
            }

            using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
            try
            {
                foreach (var product in category.Products)
                {
                    product.ChangeCategory(command.TargetCategoryId.Value);
                }

                category.MarkAsDeleted();
                
                await _dbContext.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);

                return new DeleteCategoryResult(true, $"{productCount} adet ürün {targetCategory.Name} kategorisine aktarıldı ve kategori başarıyla silindi.");
            }
            catch
            {
                await transaction.RollbackAsync(cancellationToken);
                throw;
            }
        }
        else
        {
            category.MarkAsDeleted();
            await _dbContext.SaveChangesAsync(cancellationToken);
            return new DeleteCategoryResult(true, "Kategori başarıyla silindi.");
        }
    }
}
