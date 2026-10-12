using System;

namespace ECommerce.API.Modules.Finance.Features.RejectDepositTicket;

public record RejectDepositTicketCommand(Guid TicketId, string Reason);
