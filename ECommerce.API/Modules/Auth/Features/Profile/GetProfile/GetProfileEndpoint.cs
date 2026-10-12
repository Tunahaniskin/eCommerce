using ECommerce.API.Infrastructure.Endpoints;
using ECommerce.API.Infrastructure.Handlers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using System;
using System.Security.Claims;

namespace ECommerce.API.Modules.Auth.Features.Profile.GetProfile;

public class GetProfileEndpoint : IAdminEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/profile", async (
            IQueryHandler<GetProfileQuery, ProfileResponseDto?> handler,
            HttpContext context) =>
        {
            var userIdStr = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(userIdStr, out var userId))
                return Results.Unauthorized();

            var result = await handler.HandleAsync(new GetProfileQuery(userId));
            if (result == null)
                return Results.NotFound("Kullanıcı bulunamadı.");

            return Results.Ok(result);
        })
        .RequireAuthorization()
        .WithTags("Admin - Profile")
        .Produces<ProfileResponseDto>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status404NotFound);
    }
}
