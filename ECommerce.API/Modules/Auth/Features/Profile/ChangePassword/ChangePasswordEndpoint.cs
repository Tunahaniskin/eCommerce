using ECommerce.API.Infrastructure.Endpoints;
using ECommerce.API.Infrastructure.Handlers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Security.Claims;

namespace ECommerce.API.Modules.Auth.Features.Profile.ChangePassword;

public record ChangePasswordRequest(string CurrentPassword, string NewPassword);

public class ChangePasswordEndpoint : IAdminEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/profile/change-password", async (
            [FromBody] ChangePasswordRequest request,
            ICommandHandler<ChangePasswordCommand, IResult> handler,
            HttpContext context) =>
        {
            var userIdStr = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(userIdStr, out var userId))
                return Results.Unauthorized();

            try
            {
                var command = new ChangePasswordCommand(userId, request.CurrentPassword, request.NewPassword);
                await handler.HandleAsync(command);
                return Results.Ok(new { message = "Şifreniz başarıyla güncellendi." });
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
