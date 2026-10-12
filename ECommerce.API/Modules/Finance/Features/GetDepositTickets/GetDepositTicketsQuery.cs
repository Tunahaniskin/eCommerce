using ECommerce.API.Modules.Finance.Entities;
using System;

namespace ECommerce.API.Modules.Finance.Features.GetDepositTickets;

public record DepositTicketDto(
    Guid Id, 
    Guid UserId, 
    string UserFullName,
    string UserEmail,
    decimal Amount, 
    string Status, 
    string? AdminNote, 
    DateTime CreatedAt, 
    DateTime? ProcessedAt);

public record GetDepositTicketsQuery(string? Status = null);
