using ExpenseTracker.Domain.Entities;
using FluentAssertions;

namespace ExpenseTracker.Application.Tests.Domain;

public class ExpenseTests
{
    [Fact]
    public void Create_WithValidData_ShouldCreateExpense()
    {
        // Arrange
        var amount = 100.50m;
        var description = "Groceries";
        var date = DateTime.UtcNow;
        var userId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();
        var notes = "Weekly shopping";

        // Act
        var expense = Expense.Create(amount, description, date, userId, categoryId, notes);

        // Assert
        expense.Should().NotBeNull();
        expense.Amount.Should().Be(amount);
        expense.Description.Should().Be(description);
        expense.Date.Should().Be(date);
        expense.UserId.Should().Be(userId);
        expense.CategoryId.Should().Be(categoryId);
        expense.Notes.Should().Be(notes);
        expense.Id.Should().NotBeEmpty();
        expense.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void Create_WithZeroAmount_ShouldThrowArgumentException()
    {
        // Arrange
        var amount = 0m;

        // Act
        var act = () => Expense.Create(amount, "Test", DateTime.UtcNow, Guid.NewGuid(), Guid.NewGuid());

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Amount must be greater than zero*");
    }

    [Fact]
    public void Create_WithNegativeAmount_ShouldThrowArgumentException()
    {
        // Arrange
        var amount = -50m;

        // Act
        var act = () => Expense.Create(amount, "Test", DateTime.UtcNow, Guid.NewGuid(), Guid.NewGuid());

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Amount must be greater than zero*");
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Create_WithInvalidDescription_ShouldThrowArgumentException(string? description)
    {
        // Act
        var act = () => Expense.Create(100m, description!, DateTime.UtcNow, Guid.NewGuid(), Guid.NewGuid());

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Description is required*");
    }

    [Fact]
    public void Update_WithValidData_ShouldUpdateExpense()
    {
        // Arrange
        var expense = Expense.Create(100m, "Original", DateTime.UtcNow, Guid.NewGuid(), Guid.NewGuid());
        var newAmount = 200m;
        var newDescription = "Updated";
        var newDate = DateTime.UtcNow.AddDays(-1);
        var newCategoryId = Guid.NewGuid();
        var newNotes = "Updated notes";

        // Act
        expense.Update(newAmount, newDescription, newDate, newCategoryId, newNotes);

        // Assert
        expense.Amount.Should().Be(newAmount);
        expense.Description.Should().Be(newDescription);
        expense.Date.Should().Be(newDate);
        expense.CategoryId.Should().Be(newCategoryId);
        expense.Notes.Should().Be(newNotes);
        expense.UpdatedAt.Should().NotBeNull();
        expense.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void Update_WithZeroAmount_ShouldThrowArgumentException()
    {
        // Arrange
        var expense = Expense.Create(100m, "Test", DateTime.UtcNow, Guid.NewGuid(), Guid.NewGuid());

        // Act
        var act = () => expense.Update(0m, "Test", DateTime.UtcNow, Guid.NewGuid(), null);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Amount must be greater than zero*");
    }
}
