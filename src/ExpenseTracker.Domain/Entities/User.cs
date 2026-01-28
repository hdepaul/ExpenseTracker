using ExpenseTracker.Domain.Common;

namespace ExpenseTracker.Domain.Entities;

public class User : BaseEntity
{
    public string Email { get; private set; } = null!;
    public string PasswordHash { get; private set; } = null!;
    public string FirstName { get; private set; } = null!;
    public string LastName { get; private set; } = null!;

    // Navigation properties
    private readonly List<Expense> _expenses = new();
    public IReadOnlyCollection<Expense> Expenses => _expenses.AsReadOnly();

    private readonly List<Category> _categories = new();
    public IReadOnlyCollection<Category> Categories => _categories.AsReadOnly();

    private User() { } // EF Core

    public static User Create(string email, string passwordHash, string firstName, string lastName)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email is required", nameof(email));

        if (string.IsNullOrWhiteSpace(passwordHash))
            throw new ArgumentException("Password is required", nameof(passwordHash));

        return new User
        {
            Email = email.ToLowerInvariant(),
            PasswordHash = passwordHash,
            FirstName = firstName,
            LastName = lastName
        };
    }

    public void UpdateProfile(string firstName, string lastName)
    {
        FirstName = firstName;
        LastName = lastName;
        SetUpdated();
    }

    public void UpdatePassword(string newPasswordHash)
    {
        PasswordHash = newPasswordHash;
        SetUpdated();
    }

    public string FullName => $"{FirstName} {LastName}";
}
