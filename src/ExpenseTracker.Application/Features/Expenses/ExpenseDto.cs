namespace ExpenseTracker.Application.Features.Expenses;

public record ExpenseDto(
    Guid Id,
    decimal Amount,
    string Description,
    DateTime Date,
    string? Notes,
    Guid CategoryId,
    string CategoryName,
    DateTime CreatedAt,
    DateTime? UpdatedAt);

public record ExpenseSummaryDto(
    decimal TotalAmount,
    int Count,
    Dictionary<string, decimal> ByCategory);
