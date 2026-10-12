namespace ECommerce.API.Modules.Audit.Features.GetLogs;

public record GetLogsQuery(
    int Page, 
    int PageSize, 
    string? LogType, 
    string? Level, 
    string? SearchText,
    string? Search = null,
    DateTime? StartDate = null,
    DateTime? EndDate = null
);
