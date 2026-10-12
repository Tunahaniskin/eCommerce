using ECommerce.API.Infrastructure.Database;
using ECommerce.API.Infrastructure.Handlers;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ECommerce.API.Modules.Product.Features.Categories.UpdateCategory;

public class UpdateCategoryCommandHandler : ICommandHandler<UpdateCategoryCommand, UpdateCategoryResult>
{
    private readonly AppDbContext _dbContext;

    public UpdateCategoryCommandHandler(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<UpdateCategoryResult> HandleAsync(UpdateCategoryCommand command, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(command.Name))
        {
            throw new ArgumentException("Kategori adı zorunludur.");
        }

        var category = await _dbContext.Categories
            .FirstOrDefaultAsync(c => c.Id == command.Id && !c.IsDeleted, cancellationToken);

        if (category == null)
        {
            throw new InvalidOperationException("Kategori bulunamadı.");
        }

        category.UpdateDetails(command.Name, command.Description);
        
        await _dbContext.SaveChangesAsync(cancellationToken);

        return new UpdateCategoryResult(category.Id, "Kategori başarıyla güncellendi.");
    }
}
