using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;

namespace ECommerce.API.Modules.Product.Features.Images.UploadImage;

public record UploadImageCommand(Guid ProductId, IReadOnlyList<IFormFile> Files);
public record UploadedImageDto(Guid Id, bool IsMain, int DisplayOrder);
