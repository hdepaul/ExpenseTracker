using ExpenseTracker.Application.Common.Interfaces;
using ExpenseTracker.Domain.Entities;
using ExpenseTracker.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ExpenseTracker.Application.Features.Expenses.Queries.GetExpenseById;

public class GetExpenseByIdQueryHandler : IRequestHandler<GetExpenseByIdQuery, ExpenseDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public GetExpenseByIdQueryHandler(IApplicationDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<ExpenseDto> Handle(GetExpenseByIdQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId
            ?? throw new UnauthorizedException("User must be authenticated");

        var expense = await _context.Expenses
            .AsNoTracking()
            .Include(e => e.Category)
            .Where(e => e.Id == request.Id && e.UserId == userId)
            .Select(e => new ExpenseDto(
                e.Id,
                e.Amount,
                e.Description,
                e.Date,
                e.Notes,
                e.CategoryId,
                e.Category.Name,
                e.CreatedAt,
                e.UpdatedAt))
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new NotFoundException(nameof(Expense), request.Id);

        return expense;
    }
}
