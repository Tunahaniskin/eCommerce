using ECommerce.API.Infrastructure.Database;
using ECommerce.API.Infrastructure.Handlers;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.API.Modules.Order.Features.GetOrderStatus;

public class GetOrderStatusQueryHandler : IQueryHandler<GetOrderStatusQuery, IResult>
{
    private readonly AppDbContext _dbContext;

    public GetOrderStatusQueryHandler(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IResult> HandleAsync(GetOrderStatusQuery query, CancellationToken cancellationToken = default)
    {
        if (query.OrderId == Guid.Empty)
            return Results.BadRequest(new { Message = "Geçersiz sipariş kimliği." });

        // Sadece ihtiyacımız olan alanları (Id ve Status) veritabanından çekiyoruz
        var order = await _dbContext.Orders
            .AsNoTracking()
            .Where(o => o.Id == query.OrderId)
            .Select(o => new 
            {
                o.Id,
                Status = o.Status.ToString() // Enum değerini string olarak ("Pending", "Paid", "Cancelled") alıyoruz
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (order is null)
            return Results.NotFound(new { Message = "Sipariş bulunamadı." });

        return Results.Ok(new
        {
            OrderId = order.Id,
            Status = order.Status,
            // Frontend'in sürekli istek atmasını (polling) durdurması için bir bayrak (flag)
            IsCompleted = order.Status != "Pending" 
        });
    }
}
