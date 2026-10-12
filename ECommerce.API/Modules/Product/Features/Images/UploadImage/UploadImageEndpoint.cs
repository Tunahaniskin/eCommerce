using ECommerce.API.Infrastructure.Endpoints;
using ECommerce.API.Infrastructure.Handlers;
using ECommerce.API.Modules.Auth.Constants;
using ECommerce.API.Infrastructure.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace ECommerce.API.Modules.Product.Features.Images.UploadImage;

public class UploadImageEndpoint : IAdminEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/products/{id:guid}/images", async (
            Guid id,
            HttpRequest request,
            ICommandHandler<UploadImageCommand, List<UploadedImageDto>> handler) =>
        {
            if (!request.HasFormContentType)
                return Results.BadRequest(new { message = "Lütfen form-data (multipart/form-data) olarak gönderin." });

            var form = await request.ReadFormAsync();
            var files = form.Files;

            if (files.Count == 0)
                return Results.BadRequest(new { message = "Lütfen en az bir görsel seçin." });

            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };
            foreach (var file in files)
            {
                var ext = Path.GetExtension(file.FileName).ToLower();
                if (!allowedExtensions.Contains(ext))
                {
                    return Results.BadRequest(new { message = $"{file.FileName} geçersiz dosya formatı. Sadece .jpg, .jpeg, .png ve .webp desteklenmektedir." });
                }
            }

            try
            {
                var command = new UploadImageCommand(id, files);
                var result = await handler.HandleAsync(command);
                return Results.Ok(result);
            }
            catch (ArgumentException ex)
            {
                return Results.BadRequest(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return Results.NotFound(new { message = ex.Message });
            }
        })
        .DisableAntiforgery()
        .RequirePermission(Permissions.Product.Update)
        .WithTags("Admin - Product Images");
    }
}
