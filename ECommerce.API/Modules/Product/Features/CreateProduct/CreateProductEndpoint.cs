using ECommerce.API.Infrastructure.Endpoints;
using ECommerce.API.Infrastructure.Extensions;
using ECommerce.API.Modules.Auth.Constants;
using ECommerce.API.Infrastructure.Handlers;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.API.Modules.Product.Features.CreateProduct;

public class CreateProductEndpoint : IAdminEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/products", async (
            [FromForm] string name,
            [FromForm] decimal price,
            [FromForm] int stock,
            [FromForm] Guid categoryId,
            [FromForm] IFormFileCollection? images,
            ICommandHandler<CreateProductCommand, CreateProductResult> handler) =>
        {
            if (string.IsNullOrWhiteSpace(name))
                return Results.BadRequest(new { Message = "Ürün adı boş olamaz." });
            if (name.Length < 2 || name.Length > 200)
                return Results.BadRequest(new { Message = "Ürün adı 2 ile 200 karakter arasında olmalıdır." });
            if (price <= 0 || price >= 10_000_000)
                return Results.BadRequest(new { Message = "Fiyat 0'dan büyük olmalı ve 10.000.000 sınırını aşmamalıdır." });
            if (stock < 0 || stock >= 1_000_000)
                return Results.BadRequest(new { Message = "Stok negatif olamaz ve 1.000.000 sınırını aşmamalıdır." });

            if (categoryId == Guid.Empty)
                return Results.BadRequest(new { Message = "Kategori seçimi zorunludur." });

            var command = new CreateProductCommand(name, price, stock, categoryId, images);
            var result = await handler.HandleAsync(command);

            return Results.Created($"/api/products/{result.Id}", result);
        })
        .DisableAntiforgery()
        .RequirePermission(Permissions.Product.Create);
    }
}
