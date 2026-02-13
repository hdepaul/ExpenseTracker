namespace ExpenseTracker.Application.Features.AIAgent;

public record ChatResponse(
    string Type,
    string Message,
    Guid? ExpenseId);
