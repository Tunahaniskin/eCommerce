using ECommerce.API.Infrastructure.Database;
using ECommerce.API.Infrastructure.Endpoints;
using ECommerce.API.Infrastructure.Validation;
using ECommerce.API.Modules.Catalog.Entities;

namespace ECommerce.API.Modules.Catalog.Features.CreateProduct;

public class CreateProductEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/catalog/products", async (CreateProductRequest request, AppDbContext dbContext) =>
        {
            var product = new Product(request.Name, request.Price, request.Stock);
            
            dbContext.Products.Add(product);
            await dbContext.SaveChangesAsync();

            return Results.Created($"/api/catalog/products/{product.Id}", new 
            { 
                product.Id, 
                Message = "Ürün başarıyla oluşturuldu." 
            });
        })
        .AddEndpointFilter<ValidationFilter<CreateProductRequest>>();
    }
}

public record CreateProductRequest(string Name, decimal Price, int Stock);