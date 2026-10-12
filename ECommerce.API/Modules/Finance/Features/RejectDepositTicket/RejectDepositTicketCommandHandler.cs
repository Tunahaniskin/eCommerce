using ECommerce.API.Infrastructure.Database;
using ECommerce.API.Infrastructure.Handlers;
using ECommerce.API.Modules.Finance.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using System;

namespace ECommerce.API.Modules.Finance.Features.RejectDepositTicket;

public class RejectDepositTicketCommandHandler : ICommandHandler<RejectDepositTicketCommand, IResult>
{
    private readonly AppDbContext _dbContext;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public RejectDepositTicketCommandHandler(AppDbContext dbContext, IHttpContextAccessor httpContextAccessor)
    {
        _dbContext = dbContext;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<IResult> HandleAsync(RejectDepositTicketCommand command, CancellationToken cancellationToken = default)
    {
        var adminIdString = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(adminIdString) || !Guid.TryParse(adminIdString, out var adminId))
        {
            return Results.Unauthorized();
        }

        if (string.IsNullOrWhiteSpace(command.Reason))
        {
            return Results.BadRequest(new { Message = "Ret sebebi belirtilmelidir." });
        }

        var ticket = await _dbContext.DepositTickets
            .FirstOrDefaultAsync(t => t.Id == command.TicketId, cancellationToken);

        if (ticket == null)
            return Results.NotFound(new { Message = "Bilet bulunamadı." });

        if (ticket.Status != TicketStatus.Pending)
            return Results.BadRequest(new { Message = "Yalnızca bekleyen biletler reddedilebilir." });

        ticket.Reject(adminId, command.Reason);

        await _dbContext.SaveChangesAsync(cancellationToken);

        return Results.Ok(new { Message = "Bakiye yükleme talebi reddedildi." });
    }
}
