using MediatR;

namespace ExpenseTracker.Application.Features.Budget.Commands.DeleteBudget;

public record DeleteBudgetCommand() : IRequest;
