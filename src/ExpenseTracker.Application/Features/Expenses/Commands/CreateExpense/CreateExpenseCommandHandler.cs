using ExpenseTracker.Application.Common.Interfaces;
using ExpenseTracker.Domain.Entities;
using ExpenseTracker.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ExpenseTracker.Application.Features.Expenses.Commands.CreateExpense;

public class CreateExpenseCommandHandler : IRequestHandler<CreateExpenseCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public CreateExpenseCommandHandler(IApplicationDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<Guid> Handle(CreateExpenseCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId
            ?? throw new UnauthorizedException("User must be authenticated");

        // Verify category exists and belongs to user (or is a default category)
        var categoryExists = await _context.Categories
            .AnyAsync(c => c.Id == request.CategoryId &&
                          (c.UserId == null || c.UserId == userId), cancellationToken);

        if (!categoryExists)
        {
            throw new NotFoundException(nameof(Category), request.CategoryId);
        }

        var expense = Expense.Create(
            request.Amount,
            request.Description,
            request.Date,
            userId,
            request.CategoryId,
            request.Notes);

        _context.Expenses.Add(expense);
        await _context.SaveChangesAsync(cancellationToken);

        return expense.Id;
    }
}
