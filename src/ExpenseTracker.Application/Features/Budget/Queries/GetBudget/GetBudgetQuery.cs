using MediatR;

namespace ExpenseTracker.Application.Features.Budget.Queries.GetBudget;

public record GetBudgetQuery() : IRequest<BudgetDto?>;
