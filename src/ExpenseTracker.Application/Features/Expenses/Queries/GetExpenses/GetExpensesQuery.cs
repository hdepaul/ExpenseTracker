using ExpenseTracker.Application.Common.Models;
using MediatR;

namespace ExpenseTracker.Application.Features.Expenses.Queries.GetExpenses;

public record GetExpensesQuery(
    int PageNumber = 1,
    int PageSize = 10,
    Guid? CategoryId = null,
    DateTime? FromDate = null,
    DateTime? ToDate = null) : IRequest<PagedResult<ExpenseDto>>;
