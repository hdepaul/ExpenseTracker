using ExpenseTracker.Application.Common.Interfaces;
using ExpenseTracker.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ExpenseTracker.Application.Features.Categories.Queries.GetCategories;

public class GetCategoriesQueryHandler : IRequestHandler<GetCategoriesQuery, IReadOnlyList<CategoryDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public GetCategoriesQueryHandler(IApplicationDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<IReadOnlyList<CategoryDto>> Handle(GetCategoriesQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId
            ?? throw new UnauthorizedException("User must be authenticated");

        var categories = await _context.Categories
            .AsNoTracking()
            .Where(c => c.UserId == null || c.UserId == userId) // Default + user's custom
            .OrderBy(c => c.IsDefault)
            .ThenBy(c => c.Name)
            .Select(c => new CategoryDto(
                c.Id,
                c.Name,
                c.Description,
                c.Icon,
                c.Color,
                c.IsDefault))
            .ToListAsync(cancellationToken);

        return categories;
    }
}
