using ExpenseTracker.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace ExpenseTracker.Application.Features.Admin.Queries.GetUsers;

public class GetUsersQueryHandler : IRequestHandler<GetUsersQuery, List<AdminUserDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IConfiguration _configuration;

    public GetUsersQueryHandler(IApplicationDbContext context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
    }

    public async Task<List<AdminUserDto>> Handle(GetUsersQuery request, CancellationToken cancellationToken)
    {
        var today = DateTime.UtcNow.Date;
        var dailyLimit = _configuration.GetValue<int>("Claude:DailyMessageLimit", 30);

        var users = await _context.Users
            .OrderByDescending(u => u.CreatedAt)
            .ToListAsync(cancellationToken);

        var todayUsage = await _context.AIUsageLogs
            .Where(log => log.Date == today)
            .ToDictionaryAsync(log => log.UserId, log => log.MessageCount, cancellationToken);

        return users.Select(u => new AdminUserDto(
            u.Id,
            u.Email,
            u.FirstName,
            u.LastName,
            u.CreatedAt,
            todayUsage.GetValueOrDefault(u.Id, 0),
            dailyLimit))
        .ToList();
    }
}
