using System;

namespace ECommerce.API.Modules.Product.Entities;

public class ProductImage
{
    public Guid Id { get; private set; }
    public Guid ProductId { get; private set; }
    public string FileName { get; private set; }
    public string ContentType { get; private set; }
    public string ImagePath { get; private set; }
    public string ThumbnailPath { get; private set; }
    public long FileSizeBytes { get; private set; }
    public bool IsMain { get; private set; }
    public int DisplayOrder { get; private set; }
    public DateTime CreatedAt { get; private set; }

    public Product Product { get; private set; } = default!;

    private ProductImage() { FileName = default!; ContentType = default!; ImagePath = default!; ThumbnailPath = default!; }

    public ProductImage(Guid productId, string fileName, string contentType, string imagePath, string thumbnailPath, long fileSizeBytes, bool isMain, int displayOrder)
    {
        Id = Guid.NewGuid();
        ProductId = productId;
        FileName = fileName;
        ContentType = contentType;
        ImagePath = imagePath;
        ThumbnailPath = thumbnailPath;
        FileSizeBytes = fileSizeBytes;
        IsMain = isMain;
        DisplayOrder = displayOrder;
        CreatedAt = DateTime.UtcNow;
    }

    public void SetMain(bool isMain)
    {
        IsMain = isMain;
    }

    public void SetDisplayOrder(int order)
    {
        DisplayOrder = order;
    }
}
