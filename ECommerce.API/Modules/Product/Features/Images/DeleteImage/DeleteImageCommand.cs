using System;

namespace ECommerce.API.Modules.Product.Features.Images.DeleteImage;

public record DeleteImageCommand(Guid ImageId);
public record DeleteImageResult(bool Success, string Message);
