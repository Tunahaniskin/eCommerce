using ECommerce.API.Infrastructure.Endpoints;
using ECommerce.API.Infrastructure.Extensions;
using ECommerce.API.Infrastructure.Handlers;
using ECommerce.API.Modules.Auth.Constants;

namespace ECommerce.API.Modules.Auth.Features.RoleManagement.GetUserById;

public class GetUserByIdEndpoint : IAdminEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/users/{id:guid}", async (Guid id, IQueryHandler<GetUserByIdQuery, GetUserByIdResult> handler) =>
        {
            var result = await handler.HandleAsync(new GetUserByIdQuery(id));
            return Results.Ok(result.User);
        })
        .RequirePermission(Permissions.Users.View);
    }
}
