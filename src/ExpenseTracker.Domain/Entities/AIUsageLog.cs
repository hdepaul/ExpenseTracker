using ExpenseTracker.Domain.Common;

namespace ExpenseTracker.Domain.Entities;

public class AIUsageLog : BaseEntity
{
    public Guid UserId { get; private set; }
    public DateTime Date { get; private set; }
    public int MessageCount { get; private set; }

    private AIUsageLog() { } // EF Core

    public static AIUsageLog Create(Guid userId, DateTime date)
    {
        return new AIUsageLog
        {
            UserId = userId,
            Date = date.Date,
            MessageCount = 1
        };
    }

    public void Increment()
    {
        MessageCount++;
        SetUpdated();
    }
}
