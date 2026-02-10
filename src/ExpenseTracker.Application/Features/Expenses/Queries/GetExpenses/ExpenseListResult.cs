namespace ExpenseTracker.Application.Features.Expenses.Queries.GetExpenses;

public class ExpenseListResult
{
    public IReadOnlyList<ExpenseDto> Items { get; }
    public int TotalCount { get; }
    public int PageNumber { get; }
    public int PageSize { get; }
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
    public bool HasPreviousPage => PageNumber > 1;
    public bool HasNextPage => PageNumber < TotalPages;

    // Summary across ALL expenses (not just current page)
    public decimal TotalAmount { get; }
    public IReadOnlyList<CategorySummary> ByCategory { get; }

    public ExpenseListResult(
        IReadOnlyList<ExpenseDto> items,
        int totalCount,
        int pageNumber,
        int pageSize,
        decimal totalAmount,
        IReadOnlyList<CategorySummary> byCategory)
    {
        Items = items;
        TotalCount = totalCount;
        PageNumber = pageNumber;
        PageSize = pageSize;
        TotalAmount = totalAmount;
        ByCategory = byCategory;
    }
}

public record CategorySummary(string CategoryName, decimal Amount);
