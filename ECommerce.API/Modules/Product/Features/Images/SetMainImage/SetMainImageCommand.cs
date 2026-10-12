using System;

namespace ECommerce.API.Modules.Product.Features.Images.SetMainImage;

public record SetMainImageCommand(Guid ProductId, Guid ImageId);
public record SetMainImageResult(bool Success, string Message);
