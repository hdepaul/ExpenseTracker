using ExpenseTracker.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ExpenseTracker.Infrastructure.Data;

public class ApplicationDbContextInitializer
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<ApplicationDbContextInitializer> _logger;

    public ApplicationDbContextInitializer(
        ApplicationDbContext context,
        ILogger<ApplicationDbContextInitializer> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task InitializeAsync()
    {
        try
        {
            await _context.Database.MigrateAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while migrating the database");
            throw;
        }
    }

    public async Task SeedAsync()
    {
        try
        {
            await SeedDefaultCategoriesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while seeding the database");
            throw;
        }
    }

    private async Task SeedDefaultCategoriesAsync()
    {
        if (await _context.Categories.AnyAsync())
            return;

        var defaultCategories = new[]
        {
            Category.Create("Food & Dining", "Restaurants, groceries, coffee shops", "🍔", "#FF6B6B"),
            Category.Create("Transportation", "Gas, public transit, rideshare", "🚗", "#4ECDC4"),
            Category.Create("Housing", "Rent, mortgage, utilities", "🏠", "#45B7D1"),
            Category.Create("Entertainment", "Movies, games, streaming services", "🎬", "#96CEB4"),
            Category.Create("Shopping", "Clothing, electronics, personal items", "🛒", "#FFEAA7"),
            Category.Create("Healthcare", "Medical, dental, pharmacy", "💊", "#DDA0DD"),
            Category.Create("Utilities", "Electric, water, internet, phone", "💡", "#98D8C8"),
            Category.Create("Other", "Miscellaneous expenses", "📦", "#B8B8B8")
        };

        _context.Categories.AddRange(defaultCategories);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Seeded {Count} default categories", defaultCategories.Length);
    }
}
