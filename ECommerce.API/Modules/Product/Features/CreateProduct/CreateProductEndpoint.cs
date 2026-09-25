using ECommerce.API.Infrastructure.Database;
using ECommerce.API.Infrastructure.Endpoints;
using ECommerce.API.Infrastructure.Extensions;
using ECommerce.API.Infrastructure.Validation;
using ECommerce.API.Modules.Auth.Constants;
using ProductEntity = ECommerce.API.Modules.Product.Entities.Product;

namespace ECommerce.API.Modules.Product.Features.CreateProduct;

public class CreateProductEndpoint : IAdminEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/products", async (CreateProductRequest request, AppDbContext dbContext) =>
        {
            var product = new ProductEntity(request.Name, request.Price, request.Stock);
            
            dbContext.Products.Add(product);
            await dbContext.SaveChangesAsync();

            return Results.Created($"/api/products/{product.Id}", new 
            { 
                product.Id, 
                Message = "Ürün başarıyla oluşturuldu." 
            });
        })
        .AddEndpointFilter<ValidationFilter<CreateProductRequest>>()
        .RequirePermission(Permissions.Product.Create);
    }
}

public record CreateProductRequest(string Name, decimal Price, int Stock);
