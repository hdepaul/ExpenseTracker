using ExpenseTracker.Application.Features.Expenses.Commands.CreateExpense;
using FluentAssertions;

namespace ExpenseTracker.Application.Tests.Features.Expenses.Validators;

public class CreateExpenseCommandValidatorTests
{
    private readonly CreateExpenseCommandValidator _validator;

    public CreateExpenseCommandValidatorTests()
    {
        _validator = new CreateExpenseCommandValidator();
    }

    [Fact]
    public async Task Validate_WithValidCommand_ShouldPass()
    {
        // Arrange
        var command = new CreateExpenseCommand(
            Amount: 100.50m,
            Description: "Groceries",
            Date: DateTime.UtcNow,
            CategoryId: Guid.NewGuid(),
            Notes: "Weekly shopping"
        );

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100.50)]
    public async Task Validate_WithInvalidAmount_ShouldFail(decimal amount)
    {
        // Arrange
        var command = new CreateExpenseCommand(
            Amount: amount,
            Description: "Test",
            Date: DateTime.UtcNow,
            CategoryId: Guid.NewGuid(),
            Notes: null
        );

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Amount");
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public async Task Validate_WithInvalidDescription_ShouldFail(string? description)
    {
        // Arrange
        var command = new CreateExpenseCommand(
            Amount: 100m,
            Description: description!,
            Date: DateTime.UtcNow,
            CategoryId: Guid.NewGuid(),
            Notes: null
        );

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Description");
    }

    [Fact]
    public async Task Validate_WithDescriptionTooLong_ShouldFail()
    {
        // Arrange
        var longDescription = new string('a', 201);
        var command = new CreateExpenseCommand(
            Amount: 100m,
            Description: longDescription,
            Date: DateTime.UtcNow,
            CategoryId: Guid.NewGuid(),
            Notes: null
        );

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e =>
            e.PropertyName == "Description" &&
            e.ErrorMessage.Contains("200"));
    }

    [Fact]
    public async Task Validate_WithFutureDate_ShouldFail()
    {
        // Arrange
        var command = new CreateExpenseCommand(
            Amount: 100m,
            Description: "Test",
            Date: DateTime.UtcNow.AddDays(7), // Future date
            CategoryId: Guid.NewGuid(),
            Notes: null
        );

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Date");
    }

    [Fact]
    public async Task Validate_WithEmptyCategoryId_ShouldFail()
    {
        // Arrange
        var command = new CreateExpenseCommand(
            Amount: 100m,
            Description: "Test",
            Date: DateTime.UtcNow,
            CategoryId: Guid.Empty,
            Notes: null
        );

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "CategoryId");
    }

    [Fact]
    public async Task Validate_WithNotesTooLong_ShouldFail()
    {
        // Arrange
        var longNotes = new string('a', 501);
        var command = new CreateExpenseCommand(
            Amount: 100m,
            Description: "Test",
            Date: DateTime.UtcNow,
            CategoryId: Guid.NewGuid(),
            Notes: longNotes
        );

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e =>
            e.PropertyName == "Notes" &&
            e.ErrorMessage.Contains("500"));
    }
}
