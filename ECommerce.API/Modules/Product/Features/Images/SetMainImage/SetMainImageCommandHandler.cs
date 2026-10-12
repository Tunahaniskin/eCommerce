using ECommerce.API.Infrastructure.Database;
using ECommerce.API.Infrastructure.Handlers;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ECommerce.API.Modules.Product.Features.Images.SetMainImage;

public class SetMainImageCommandHandler : ICommandHandler<SetMainImageCommand, SetMainImageResult>
{
    private readonly AppDbContext _dbContext;

    public SetMainImageCommandHandler(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<SetMainImageResult> HandleAsync(SetMainImageCommand command, CancellationToken cancellationToken = default)
    {
        var images = await _dbContext.ProductImages
            .Where(i => i.ProductId == command.ProductId)
            .ToListAsync(cancellationToken);

        if (!images.Any())
            throw new InvalidOperationException("Ürüne ait görsel bulunamadı.");

        var targetImage = images.FirstOrDefault(i => i.Id == command.ImageId);
        if (targetImage == null)
            throw new InvalidOperationException("Belirtilen görsel bulunamadı.");

        foreach (var image in images)
        {
            image.SetMain(image.Id == command.ImageId);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        return new SetMainImageResult(true, "Ana görsel başarıyla güncellendi.");
    }
}
