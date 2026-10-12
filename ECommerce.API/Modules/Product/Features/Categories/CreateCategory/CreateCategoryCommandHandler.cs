using ECommerce.API.Infrastructure.Database;
using ECommerce.API.Infrastructure.Handlers;
using ECommerce.API.Modules.Product.Entities;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ECommerce.API.Modules.Product.Features.Categories.CreateCategory;

public class CreateCategoryCommandHandler : ICommandHandler<CreateCategoryCommand, CreateCategoryResult>
{
    private readonly AppDbContext _dbContext;

    public CreateCategoryCommandHandler(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<CreateCategoryResult> HandleAsync(CreateCategoryCommand command, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(command.Name))
        {
            throw new ArgumentException("Kategori adı zorunludur.");
        }

        var category = new Category(command.Name, command.Description);
        _dbContext.Categories.Add(category);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return new CreateCategoryResult(category.Id, "Kategori başarıyla oluşturuldu.");
    }
}
