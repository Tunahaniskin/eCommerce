using ECommerce.API.Infrastructure.Database;
using ECommerce.API.Infrastructure.Endpoints;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.API.Modules.Order.Features.GetOrderStatus;

public class GetOrderStatusEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/orders/{id:guid}/status", async (Guid id, AppDbContext dbContext) =>
        {
            if (id == Guid.Empty)
                return Results.BadRequest(new { Message = "Geçersiz sipariş kimliği." });

            // Sadece ihtiyacımız olan alanları (Id ve Status) veritabanından çekiyoruz
            var order = await dbContext.Orders
                .AsNoTracking()
                .Where(o => o.Id == id)
                .Select(o => new 
                {
                    o.Id,
                    Status = o.Status.ToString() // Enum değerini string olarak ("Pending", "Paid", "Cancelled") alıyoruz
                })
                .FirstOrDefaultAsync();

            if (order is null)
                return Results.NotFound(new { Message = "Sipariş bulunamadı." });

            return Results.Ok(new
            {
                OrderId = order.Id,
                Status = order.Status,
                // Frontend'in sürekli istek atmasını (polling) durdurması için bir bayrak (flag)
                IsCompleted = order.Status != "Pending" 
            });
        });
    }
}