using ExpenseTracker.Application.Common.Interfaces;
using ExpenseTracker.Application.Common.Models;
using ExpenseTracker.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ExpenseTracker.Application.Features.Expenses.Queries.GetExpenses;

public class GetExpensesQueryHandler : IRequestHandler<GetExpensesQuery, PagedResult<ExpenseDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public GetExpensesQueryHandler(IApplicationDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<PagedResult<ExpenseDto>> Handle(GetExpensesQuery request, CancellationToken cancellationToken)
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

        return new PagedResult<ExpenseDto>(items, totalCount, request.PageNumber, request.PageSize);
    }
}
