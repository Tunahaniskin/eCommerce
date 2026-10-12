using ECommerce.API.Infrastructure.Database;
using ECommerce.API.Infrastructure.Handlers;
using ECommerce.API.Infrastructure.Storage;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ECommerce.API.Modules.Product.Features.Images.DeleteImage;

public class DeleteImageCommandHandler : ICommandHandler<DeleteImageCommand, DeleteImageResult>
{
    private readonly AppDbContext _dbContext;
    private readonly IFileStorageService _fileStorage;

    public DeleteImageCommandHandler(AppDbContext dbContext, IFileStorageService fileStorage)
    {
        _dbContext = dbContext;
        _fileStorage = fileStorage;
    }

    public async Task<DeleteImageResult> HandleAsync(DeleteImageCommand command, CancellationToken cancellationToken = default)
    {
        var image = await _dbContext.ProductImages
            .FirstOrDefaultAsync(i => i.Id == command.ImageId, cancellationToken);

        if (image == null)
            throw new InvalidOperationException("Görsel bulunamadı.");

        _dbContext.ProductImages.Remove(image);
        await _dbContext.SaveChangesAsync(cancellationToken);

        // Diskteki fiziksel dosyaları sil
        if (!string.IsNullOrEmpty(image.ImagePath))
            await _fileStorage.DeleteFileAsync(image.ImagePath);
            
        if (!string.IsNullOrEmpty(image.ThumbnailPath))
            await _fileStorage.DeleteFileAsync(image.ThumbnailPath);

        return new DeleteImageResult(true, "Görsel başarıyla silindi.");
    }
}
