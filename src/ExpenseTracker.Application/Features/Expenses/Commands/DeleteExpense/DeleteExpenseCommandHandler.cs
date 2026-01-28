using ExpenseTracker.Application.Common.Interfaces;
using ExpenseTracker.Domain.Entities;
using ExpenseTracker.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ExpenseTracker.Application.Features.Expenses.Commands.DeleteExpense;

public class DeleteExpenseCommandHandler : IRequestHandler<DeleteExpenseCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public DeleteExpenseCommandHandler(IApplicationDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task Handle(DeleteExpenseCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId
            ?? throw new UnauthorizedException("User must be authenticated");

        var expense = await _context.Expenses
            .FirstOrDefaultAsync(e => e.Id == request.Id && e.UserId == userId, cancellationToken)
            ?? throw new NotFoundException(nameof(Expense), request.Id);

        _context.Expenses.Remove(expense);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
