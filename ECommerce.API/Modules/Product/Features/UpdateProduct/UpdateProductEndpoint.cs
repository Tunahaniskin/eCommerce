using ECommerce.API.Infrastructure.Endpoints;
using ECommerce.API.Infrastructure.Extensions;
using ECommerce.API.Modules.Auth.Constants;
using ECommerce.API.Infrastructure.Handlers;

namespace ECommerce.API.Modules.Product.Features.UpdateProduct;

public class UpdateProductEndpoint : IAdminEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut("/products/{id:guid}", async (Guid id, UpdateProductRequest request, ICommandHandler<UpdateProductCommand, UpdateProductResult> handler) =>
        {
            if (string.IsNullOrWhiteSpace(request.Name))
                return Results.BadRequest(new { Message = "Ürün adı boş olamaz." });
            if (request.Name.Length < 2 || request.Name.Length > 200)
                return Results.BadRequest(new { Message = "Ürün adı 2 ile 200 karakter arasında olmalıdır." });
            if (request.Price <= 0 || request.Price >= 10_000_000)
                return Results.BadRequest(new { Message = "Fiyat 0'dan büyük olmalı ve 10.000.000 sınırını aşmamalıdır." });
            if (request.Stock < 0 || request.Stock >= 1_000_000)
                return Results.BadRequest(new { Message = "Stok negatif olamaz ve 1.000.000 sınırını aşmamalıdır." });

            if (request.CategoryId == Guid.Empty)
                return Results.BadRequest(new { Message = "Kategori seçimi zorunludur." });

            var command = new UpdateProductCommand(id, request.Name, request.Price, request.Stock, request.CategoryId);
            var result = await handler.HandleAsync(command);

            return Results.Ok(result);
        })
        .RequirePermission(Permissions.Product.Update);
    }
}

public record UpdateProductRequest(string Name, decimal Price, int Stock, Guid CategoryId);
