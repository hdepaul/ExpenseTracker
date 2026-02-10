using ExpenseTracker.Application.Common.Interfaces;
using ExpenseTracker.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ExpenseTracker.Application.Features.Expenses.Queries.GetExpenses;

public class GetExpensesQueryHandler : IRequestHandler<GetExpensesQuery, ExpenseListResult>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public GetExpensesQueryHandler(IApplicationDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<ExpenseListResult> Handle(GetExpensesQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId
            ?? throw new UnauthorizedException("User must be authenticated");

        var query = _context.Expenses
            .AsNoTracking()
            .Where(e => e.UserId == userId);

        // Apply filters
        if (request.CategoryId.HasValue)
        {
            query = query.Where(e => e.CategoryId == request.CategoryId.Value);
        }

        if (request.FromDate.HasValue)
        {
            query = query.Where(e => e.Date >= request.FromDate.Value);
        }

        if (request.ToDate.HasValue)
        {
            query = query.Where(e => e.Date <= request.ToDate.Value);
        }

        // Get total count
        var totalCount = await query.CountAsync(cancellationToken);

        // Get total amount across all filtered expenses
        var totalAmount = await query.SumAsync(e => e.Amount, cancellationToken);

        // Get summary by category
        // First get the filtered expenses with category names, then group in memory
        var expensesWithCategory = await query
            .Select(e => new { e.Amount, e.Category.Name })
            .ToListAsync(cancellationToken);

        var byCategory = expensesWithCategory
            .GroupBy(x => x.Name)
            .Select(g => new CategorySummary(g.Key, g.Sum(x => x.Amount)))
            .OrderByDescending(c => c.Amount)
            .ToList();

        // Get paged items
        var items = await query
            .Include(e => e.Category)
            .OrderByDescending(e => e.Date)
            .ThenByDescending(e => e.CreatedAt)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
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
            .ToListAsync(cancellationToken);

        return new ExpenseListResult(
            items,
            totalCount,
            request.PageNumber,
            request.PageSize,
            totalAmount,
            byCategory);
    }
}
