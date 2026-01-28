using ExpenseTracker.Application.Common.Interfaces;
using ExpenseTracker.Domain.Entities;
using ExpenseTracker.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ExpenseTracker.Application.Features.Expenses.Commands.UpdateExpense;

public class UpdateExpenseCommandHandler : IRequestHandler<UpdateExpenseCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public UpdateExpenseCommandHandler(IApplicationDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task Handle(UpdateExpenseCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId
            ?? throw new UnauthorizedException("User must be authenticated");

        var expense = await _context.Expenses
            .FirstOrDefaultAsync(e => e.Id == request.Id && e.UserId == userId, cancellationToken)
            ?? throw new NotFoundException(nameof(Expense), request.Id);

        // Verify category exists and belongs to user (or is a default category)
        var categoryExists = await _context.Categories
            .AnyAsync(c => c.Id == request.CategoryId &&
                          (c.UserId == null || c.UserId == userId), cancellationToken);

        if (!categoryExists)
        {
            throw new NotFoundException(nameof(Category), request.CategoryId);
        }

        expense.Update(
            request.Amount,
            request.Description,
            request.Date,
            request.CategoryId,
            request.Notes);

        await _context.SaveChangesAsync(cancellationToken);
    }
}
