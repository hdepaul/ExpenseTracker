using ExpenseTracker.Application.Common.Interfaces;
using ExpenseTracker.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ExpenseTracker.Application.Features.Budget.Commands.SetBudget;

public class SetBudgetCommandHandler : IRequestHandler<SetBudgetCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public SetBudgetCommandHandler(IApplicationDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task Handle(SetBudgetCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId
            ?? throw new UnauthorizedException("User must be authenticated");

        var budget = await _context.Budgets
            .FirstOrDefaultAsync(b => b.UserId == userId, cancellationToken);

        if (budget is not null)
        {
            budget.Update(request.Amount);
        }
        else
        {
            budget = Domain.Entities.Budget.Create(userId, request.Amount);
            _context.Budgets.Add(budget);
        }

        await _context.SaveChangesAsync(cancellationToken);
    }
}
