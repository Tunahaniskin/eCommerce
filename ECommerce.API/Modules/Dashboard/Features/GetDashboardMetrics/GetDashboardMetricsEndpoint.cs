using ECommerce.API.Infrastructure.Endpoints;
using ECommerce.API.Infrastructure.Handlers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace ECommerce.API.Modules.Dashboard.Features.GetDashboardMetrics;

public class GetDashboardMetricsEndpoint : IAdminEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/dashboard/metrics", async (
            IQueryHandler<GetDashboardMetricsQuery, DashboardMetricsDto> handler) =>
        {
            var query = new GetDashboardMetricsQuery();
            var result = await handler.HandleAsync(query);
            return Results.Ok(result);
        })
        .RequireAuthorization()
        .WithTags("Admin - Dashboard")
        .Produces<DashboardMetricsDto>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status401Unauthorized);
    }
}
