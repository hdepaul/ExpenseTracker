using MediatR;

namespace ExpenseTracker.Application.Features.Admin.Queries.GetUsers;

public record GetUsersQuery() : IRequest<List<AdminUserDto>>;
