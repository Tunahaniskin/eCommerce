using ECommerce.API.Infrastructure.Database;
using ECommerce.API.Infrastructure.Handlers;
using ECommerce.API.Modules.Finance.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using System;

namespace ECommerce.API.Modules.Finance.Features.ApproveDepositTicket;

public class ApproveDepositTicketCommandHandler : ICommandHandler<ApproveDepositTicketCommand, IResult>
{
    private readonly AppDbContext _dbContext;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public ApproveDepositTicketCommandHandler(AppDbContext dbContext, IHttpContextAccessor httpContextAccessor)
    {
        _dbContext = dbContext;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<IResult> HandleAsync(ApproveDepositTicketCommand command, CancellationToken cancellationToken = default)
    {
        var adminIdString = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(adminIdString) || !Guid.TryParse(adminIdString, out var adminId))
        {
            return Results.Unauthorized();
        }

        var ticket = await _dbContext.DepositTickets
            .FirstOrDefaultAsync(t => t.Id == command.TicketId, cancellationToken);

        if (ticket == null)
            return Results.NotFound(new { Message = "Bilet bulunamadı." });

        if (ticket.Status != TicketStatus.Pending)
            return Results.BadRequest(new { Message = "Yalnızca bekleyen biletler onaylanabilir." });

        var wallet = await _dbContext.Wallets
            .FirstOrDefaultAsync(w => w.UserId == ticket.UserId, cancellationToken);

        if (wallet == null)
        {
            wallet = new Wallet(ticket.UserId, "TL");
            await _dbContext.Wallets.AddAsync(wallet, cancellationToken);
        }

        wallet.AddBalance(ticket.Amount);
        
        var transaction = new WalletTransaction(wallet.Id, ticket.Amount, WalletTransactionType.Deposit, $"Admin onayı ile {ticket.Amount} TL yatırıldı.");
        await _dbContext.WalletTransactions.AddAsync(transaction, cancellationToken);

        ticket.Approve(adminId);

        await _dbContext.SaveChangesAsync(cancellationToken);

        return Results.Ok(new { Message = "Bakiye yükleme talebi onaylandı.", NewBalance = wallet.Balance });
    }
}
