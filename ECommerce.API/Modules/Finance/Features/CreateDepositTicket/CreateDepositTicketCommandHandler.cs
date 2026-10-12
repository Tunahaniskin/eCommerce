using ECommerce.API.Infrastructure.Database;
using ECommerce.API.Infrastructure.Handlers;
using ECommerce.API.Modules.Finance.Entities;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using System;

namespace ECommerce.API.Modules.Finance.Features.CreateDepositTicket;

public class CreateDepositTicketCommandHandler : ICommandHandler<CreateDepositTicketCommand, IResult>
{
    private readonly AppDbContext _dbContext;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CreateDepositTicketCommandHandler(AppDbContext dbContext, IHttpContextAccessor httpContextAccessor)
    {
        _dbContext = dbContext;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<IResult> HandleAsync(CreateDepositTicketCommand command, CancellationToken cancellationToken = default)
    {
        var userIdString = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out var userId))
        {
            return Results.Unauthorized();
        }

        if (command.Amount <= 0)
        {
            return Results.BadRequest(new { Message = "Yüklenecek tutar sıfırdan büyük olmalıdır." });
        }

        var ticket = new DepositTicket(userId, command.Amount);
        
        await _dbContext.DepositTickets.AddAsync(ticket, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return Results.Ok(new { Message = "Bakiye yükleme talebiniz alındı.", TicketId = ticket.Id });
    }
}
