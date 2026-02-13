namespace ExpenseTracker.Application.Features.Auth;

public record AuthResponse(
    string Token,
    string Email,
    string FirstName,
    string LastName,
    DateTime ExpiresAt,
    string Role);

public record UserDto(
    Guid Id,
    string Email,
    string FirstName,
    string LastName,
    DateTime CreatedAt);
