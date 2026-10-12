using ECommerce.API.Infrastructure.Database;
using ECommerce.API.Infrastructure.Handlers;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ECommerce.API.Modules.Order.Features.GetOrderById;

public class GetOrderByIdQueryHandler : IQueryHandler<GetOrderByIdQuery, OrderDetailsDto?>
{
    private readonly AppDbContext _dbContext;

    public GetOrderByIdQueryHandler(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<OrderDetailsDto?> HandleAsync(GetOrderByIdQuery query, CancellationToken cancellationToken = default)
    {
        var orderData = await (from o in _dbContext.Orders.Include(o => o.Items)
                               join u in _dbContext.Users.IgnoreQueryFilters() on o.UserId equals u.Id into userGroup
                               from u in userGroup.DefaultIfEmpty()
                               where o.Id == query.Id
                               select new
                               {
                                   Order = o,
                                   User = u,
                                   Items = o.Items
                               }).FirstOrDefaultAsync(cancellationToken);

        if (orderData == null)
            return null;

        var productIds = orderData.Items.Select(i => i.ProductId).ToList();
        var products = await _dbContext.Products
            .IgnoreQueryFilters()
            .Where(p => productIds.Contains(p.Id))
            .ToDictionaryAsync(p => p.Id, p => p.Name, cancellationToken);

        var itemsDto = orderData.Items.Select(i => new OrderDetailsItemDto(
            i.ProductId,
            products.ContainsKey(i.ProductId) ? products[i.ProductId] : "Bilinmeyen Ürün",
            i.Quantity,
            i.UnitPrice,
            i.Quantity * i.UnitPrice
        )).ToList();

        var customerEmail = orderData.User != null ? orderData.User.Email : "Bilinmiyor";
        var customerName = orderData.User != null ? $"{orderData.User.FirstName} {orderData.User.LastName}".Trim() : "Bilinmiyor";
        var customerUserId = orderData.User != null ? orderData.User.Id : orderData.Order.UserId;

        return new OrderDetailsDto(
            orderData.Order.Id,
            customerUserId,
            customerEmail,
            string.IsNullOrWhiteSpace(customerName) ? "Bilinmiyor" : customerName,
            orderData.Order.TotalAmount,
            orderData.Order.Status.ToString(),
            orderData.Order.CreatedAt,
            "Sistemde kayıtlı değil", // ShippingAddress is not currently persisted
            itemsDto
        );
    }
}
