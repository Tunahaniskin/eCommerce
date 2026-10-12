using ECommerce.API.Infrastructure.Endpoints;
using ECommerce.API.Infrastructure.Extensions;
using ECommerce.API.Modules.Auth.Constants;
using ECommerce.API.Infrastructure.Handlers;

namespace ECommerce.API.Modules.Auth.Features.RoleManagement.GetUsers;

public class GetUsersEndpoint : IAdminEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/users", async (bool? includeDeleted, IQueryHandler<GetUsersQuery, GetUsersResult> handler) =>
        {
            var result = await handler.HandleAsync(new GetUsersQuery(includeDeleted ?? false));
            return Results.Ok(result.Users);
        })
        .RequirePermission(Permissions.Users.View);
    }
}
