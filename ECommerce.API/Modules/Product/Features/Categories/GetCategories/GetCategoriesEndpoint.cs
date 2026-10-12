using ECommerce.API.Infrastructure.Endpoints;
using ECommerce.API.Infrastructure.Handlers;
using ECommerce.API.Infrastructure.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ECommerce.API.Modules.Product.Features.Categories.GetCategories;

public class GetCategoriesEndpoint : IAdminEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/categories", async ([AsParameters] GetCategoriesQuery query, IQueryHandler<GetCategoriesQuery, PagedResult<CategoryDto>> handler) =>
        {
            var result = await handler.HandleAsync(query);
            return Results.Ok(result);
        })
        .RequireAuthorization()
        .WithTags("Admin - Categories")
        .Produces<PagedResult<CategoryDto>>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status401Unauthorized);
    }
}
