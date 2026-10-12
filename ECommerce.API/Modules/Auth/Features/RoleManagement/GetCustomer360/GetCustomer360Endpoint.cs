using ECommerce.API.Infrastructure.Endpoints;
using ECommerce.API.Infrastructure.Handlers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System;

namespace ECommerce.API.Modules.Auth.Features.RoleManagement.GetCustomer360;

public class GetCustomer360Endpoint : IAdminEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/users/{userId:guid}/customer-360", async (
            Guid userId,
            IQueryHandler<GetCustomer360Query, Customer360ResponseDto?> handler) =>
        {
            var query = new GetCustomer360Query(userId);
            var result = await handler.HandleAsync(query);
            
            if (result == null)
                return Results.NotFound(new { message = "Kullanıcı bulunamadı." });
                
            return Results.Ok(result);
        })
        .RequireAuthorization()
        .WithTags("Admin - Users")
        .Produces<Customer360ResponseDto>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound)
        .Produces(StatusCodes.Status401Unauthorized);
    }
}
