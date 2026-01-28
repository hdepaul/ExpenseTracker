using ExpenseTracker.Domain.Entities;

namespace ExpenseTracker.Application.Common.Interfaces;

public interface IJwtService
{
    string GenerateToken(User user);
    string GenerateRefreshToken();
    bool ValidateToken(string token);
}
