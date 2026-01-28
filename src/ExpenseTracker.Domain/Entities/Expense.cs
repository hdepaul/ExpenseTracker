using ExpenseTracker.Domain.Common;

namespace ExpenseTracker.Domain.Entities;

public class Expense : BaseEntity
{
    public decimal Amount { get; private set; }
    public string Description { get; private set; } = null!;
    public DateTime Date { get; private set; }
    public string? Notes { get; private set; }

    // Foreign keys
    public Guid UserId { get; private set; }
    public User User { get; private set; } = null!;

    public Guid CategoryId { get; private set; }
    public Category Category { get; private set; } = null!;

    private Expense() { } // EF Core

    public static Expense Create(
        decimal amount,
        string description,
        DateTime date,
        Guid userId,
        Guid categoryId,
        string? notes = null)
    {
        if (amount <= 0)
            throw new ArgumentException("Amount must be greater than zero", nameof(amount));

        if (string.IsNullOrWhiteSpace(description))
            throw new ArgumentException("Description is required", nameof(description));

        return new Expense
        {
            Amount = amount,
            Description = description,
            Date = date,
            UserId = userId,
            CategoryId = categoryId,
            Notes = notes
        };
    }

    public void Update(decimal amount, string description, DateTime date, Guid categoryId, string? notes)
    {
        if (amount <= 0)
            throw new ArgumentException("Amount must be greater than zero", nameof(amount));

        Amount = amount;
        Description = description;
        Date = date;
        CategoryId = categoryId;
        Notes = notes;
        SetUpdated();
    }
}
