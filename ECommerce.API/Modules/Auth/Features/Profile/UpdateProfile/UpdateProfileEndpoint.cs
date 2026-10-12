using ECommerce.API.Infrastructure.Endpoints;
using ECommerce.API.Infrastructure.Handlers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Security.Claims;

namespace ECommerce.API.Modules.Auth.Features.Profile.UpdateProfile;

public record UpdateProfileRequest(string FirstName, string LastName, string Email);

public class UpdateProfileEndpoint : IAdminEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut("/profile", async (
            [FromBody] UpdateProfileRequest request,
            ICommandHandler<UpdateProfileCommand, IResult> handler,
            HttpContext context) =>
        {
            var userIdStr = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(userIdStr, out var userId))
                return Results.Unauthorized();

            try
            {
                var command = new UpdateProfileCommand(userId, request.FirstName, request.LastName, request.Email);
                await handler.HandleAsync(command);
                return Results.Ok(new { message = "Profil başarıyla güncellendi." });
            }
            catch (ArgumentException ex)
            {
                return Results.BadRequest(new { message = ex.Message, error = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return Results.BadRequest(new { message = ex.Message, error = ex.Message });
            }
        })
        .RequireAuthorization()
        .WithTags("Admin - Profile")
        .Produces(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status401Unauthorized);
    }
}
