using ExpenseTracker.Domain.Common;

namespace ExpenseTracker.Domain.Entities;

public class Category : BaseEntity
{
    public string Name { get; private set; } = null!;
    public string? Description { get; private set; }
    public string? Icon { get; private set; }
    public string? Color { get; private set; }
    public bool IsDefault { get; private set; }

    // Foreign key
    public Guid? UserId { get; private set; }
    public User? User { get; private set; }

    // Navigation
    private readonly List<Expense> _expenses = new();
    public IReadOnlyCollection<Expense> Expenses => _expenses.AsReadOnly();

    private Category() { } // EF Core

    public static Category Create(string name, string? description = null, string? icon = null, string? color = null, Guid? userId = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Category name is required", nameof(name));

        return new Category
        {
            Name = name,
            Description = description,
            Icon = icon,
            Color = color,
            UserId = userId,
            IsDefault = userId == null // Categories without user are default/system categories
        };
    }

    public void Update(string name, string? description, string? icon, string? color)
    {
        Name = name;
        Description = description;
        Icon = icon;
        Color = color;
        SetUpdated();
    }
}
