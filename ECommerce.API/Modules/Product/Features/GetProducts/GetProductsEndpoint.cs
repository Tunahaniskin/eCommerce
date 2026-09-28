using ECommerce.API.Infrastructure.Endpoints;
using ECommerce.API.Infrastructure.Extensions;
using ECommerce.API.Modules.Auth.Constants;
using ECommerce.API.Infrastructure.Handlers;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.Security.Claims;

namespace ECommerce.API.Modules.Product.Features.GetProducts;

public class GetProductsEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/products", async (
            [AsParameters] GetProductsRequest request, 
            HttpContext httpContext,
            IQueryHandler<GetProductsQuery, List<ProductResponse>> handler) =>
        {
            var authResult = await httpContext.AuthenticateAsync(JwtBearerDefaults.AuthenticationScheme);
            var user = authResult.Principal ?? httpContext.User;
            bool canReadDeleted = user.HasPermission(Permissions.Product.ReadDeleted);

            var statusKey = request.Status?.ToLower() ?? "active";

            if ((statusKey == "deleted" || statusKey == "all") && !canReadDeleted)
            {
                return Results.Problem(
                    statusCode: StatusCodes.Status403Forbidden,
                    title: "Erişim Reddedildi",
                    detail: "Silinmiş ürünleri listeleme yetkiniz bulunmuyor.");
            }

            var query = new GetProductsQuery(request.Page, request.PageSize, request.Status, canReadDeleted);
            var products = await handler.HandleAsync(query);

            return Results.Ok(products);
        });
    }
}

public record GetProductsRequest(int Page = 1, int PageSize = 10, string? Status = "active");

public record ProductResponse(Guid Id, string Name, decimal Price, int Stock, bool IsDeleted);
