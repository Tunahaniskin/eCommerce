using ECommerce.API.Infrastructure.Database;
using ECommerce.API.Infrastructure.Handlers;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ECommerce.API.Modules.Order.Features.GetOrders;

public class GetOrdersQueryHandler : IQueryHandler<GetOrdersQuery, List<OrderDto>>
{
    private readonly AppDbContext _dbContext;

    public GetOrdersQueryHandler(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<OrderDto>> HandleAsync(GetOrdersQuery query, CancellationToken cancellationToken = default)
    {
        var dbQuery = from o in _dbContext.Orders
                      join u in _dbContext.Users.IgnoreQueryFilters() on o.UserId equals u.Id into userGroup
                      from u in userGroup.DefaultIfEmpty()
                      select new { Order = o, User = u };

        if (query.UserId.HasValue)
        {
            dbQuery = dbQuery.Where(x => x.Order.UserId == query.UserId.Value);
        }

        if (!string.IsNullOrWhiteSpace(query.Status))
        {
            if (System.Enum.TryParse<ECommerce.API.Modules.Order.Entities.OrderStatus>(query.Status, true, out var orderStatus))
            {
                dbQuery = dbQuery.Where(x => x.Order.Status == orderStatus);
            }
        }

        if (query.StartDate.HasValue)
        {
            dbQuery = dbQuery.Where(x => x.Order.CreatedAt >= query.StartDate.Value);
        }

        if (query.EndDate.HasValue)
        {
            dbQuery = dbQuery.Where(x => x.Order.CreatedAt <= query.EndDate.Value);
        }

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.ToLower();
            dbQuery = dbQuery.Where(x => 
                x.Order.Id.ToString().ToLower().Contains(search) || 
                (x.User != null && x.User.Email.ToLower().Contains(search)) || 
                (x.User != null && x.User.FirstName.ToLower().Contains(search)) || 
                (x.User != null && x.User.LastName.ToLower().Contains(search)));
        }

        var resultList = await dbQuery
            .OrderByDescending(x => x.Order.CreatedAt)
            .Select(x => new OrderDto(
                x.Order.Id,
                x.User != null ? x.User.Email : "Bilinmiyor",
                x.Order.TotalAmount,
                x.Order.Status.ToString(),
                x.Order.CreatedAt
            ))
            .ToListAsync(cancellationToken);

        return resultList;
    }
}
