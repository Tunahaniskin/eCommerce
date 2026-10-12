using ECommerce.API.Infrastructure.Database;
using ECommerce.API.Infrastructure.Handlers;
using ECommerce.API.Modules.Finance.Entities;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System;

namespace ECommerce.API.Modules.Finance.Features.GetDepositTickets;

public class GetDepositTicketsQueryHandler : IQueryHandler<GetDepositTicketsQuery, List<DepositTicketDto>>
{
    private readonly AppDbContext _dbContext;

    public GetDepositTicketsQueryHandler(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<DepositTicketDto>> HandleAsync(GetDepositTicketsQuery query, CancellationToken cancellationToken = default)
    {
        var dbQuery = _dbContext.DepositTickets.AsNoTracking().AsQueryable();

        if (!string.IsNullOrEmpty(query.Status) && Enum.TryParse<TicketStatus>(query.Status, true, out var statusEnum))
        {
            dbQuery = dbQuery.Where(t => t.Status == statusEnum);
        }

        var tickets = await dbQuery
            .OrderByDescending(t => t.CreatedAt)
            .Select(t => new
            {
                t.Id,
                t.UserId,
                t.Amount,
                t.Status,
                t.AdminNote,
                t.CreatedAt,
                t.ProcessedAt
            })
            .ToListAsync(cancellationToken);

        var userIds = tickets.Select(t => t.UserId).Distinct().ToList();
        var users = await _dbContext.Users
            .AsNoTracking()
            .Where(u => userIds.Contains(u.Id))
            .ToDictionaryAsync(u => u.Id, u => new { u.FirstName, u.LastName, u.Email }, cancellationToken);

        return tickets.Select(t => new DepositTicketDto(
            t.Id,
            t.UserId,
            users.ContainsKey(t.UserId) ? $"{users[t.UserId].FirstName} {users[t.UserId].LastName}" : "Unknown",
            users.ContainsKey(t.UserId) ? users[t.UserId].Email : "Unknown",
            t.Amount,
            t.Status.ToString(),
            t.AdminNote,
            t.CreatedAt,
            t.ProcessedAt
        )).ToList();
    }
}
