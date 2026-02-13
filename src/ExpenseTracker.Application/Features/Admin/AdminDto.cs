namespace ExpenseTracker.Application.Features.Admin;

public record AdminUserDto(
    Guid UserId,
    string Email,
    string FirstName,
    string LastName,
    DateTime CreatedAt,
    int TodayMessageCount,
    int DailyMessageLimit);
