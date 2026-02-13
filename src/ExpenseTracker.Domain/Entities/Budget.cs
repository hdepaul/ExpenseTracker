using ExpenseTracker.Domain.Common;

namespace ExpenseTracker.Domain.Entities;

public class Budget : BaseEntity
{
    public Guid UserId { get; private set; }
    public User User { get; private set; } = null!;
    public decimal Amount { get; private set; }

    private Budget() { } // EF Core

    public static Budget Create(Guid userId, decimal amount)
    {
        if (amount <= 0)
            throw new ArgumentException("Amount must be greater than zero", nameof(amount));

        return new Budget
        {
            UserId = userId,
            Amount = amount
        };
    }

    public void Update(decimal amount)
    {
        if (amount <= 0)
            throw new ArgumentException("Amount must be greater than zero", nameof(amount));

        Amount = amount;
        SetUpdated();
    }
}
