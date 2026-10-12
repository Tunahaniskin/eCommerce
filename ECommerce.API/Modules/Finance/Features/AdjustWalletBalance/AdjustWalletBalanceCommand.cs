using ECommerce.API.Infrastructure.Handlers;
using System;

namespace ECommerce.API.Modules.Finance.Features.AdjustWalletBalance;

public record AdjustWalletBalanceCommand(
    Guid UserId,
    decimal Amount,
    string ActionType, // Deposit or Deduct
    string Reason,
    Guid AdminUserId
);
