using ExpenseTracker.Application.Common.Interfaces;
using ExpenseTracker.Domain.Entities;
using ExpenseTracker.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ExpenseTracker.Application.Features.Auth.Commands.Register;

public class RegisterCommandHandler : IRequestHandler<RegisterCommand, AuthResponse>
{
    private readonly IApplicationDbContext _context;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtService _jwtService;

    public RegisterCommandHandler(
        IApplicationDbContext context,
        IPasswordHasher passwordHasher,
        IJwtService jwtService)
    {
        _context = context;
        _passwordHasher = passwordHasher;
        _jwtService = jwtService;
    }

    public async Task<AuthResponse> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        // Check if email already exists
        var emailExists = await _context.Users
            .AnyAsync(u => u.Email.ToLower() == request.Email.ToLower(), cancellationToken);

        if (emailExists)
        {
            throw new ValidationException("Email", "Email is already registered");
        }

        // Hash password
        var passwordHash = _passwordHasher.Hash(request.Password);

        // Create user
        var user = User.Create(
            request.Email,
            passwordHash,
            request.FirstName,
            request.LastName);

        _context.Users.Add(user);
        await _context.SaveChangesAsync(cancellationToken);

        // Generate token
        var token = _jwtService.GenerateToken(user);

        return new AuthResponse(
            token,
            user.Email,
            user.FirstName,
            user.LastName,
            DateTime.UtcNow.AddHours(1),
            user.Role);
    }
}
