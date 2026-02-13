using MediatR;

namespace ExpenseTracker.Application.Features.Budget.Commands.SetBudget;

public record SetBudgetCommand(decimal Amount) : IRequest;
