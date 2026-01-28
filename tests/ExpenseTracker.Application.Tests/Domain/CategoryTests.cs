using ExpenseTracker.Domain.Entities;
using FluentAssertions;

namespace ExpenseTracker.Application.Tests.Domain;

public class CategoryTests
{
    [Fact]
    public void Create_WithValidData_ShouldCreateCategory()
    {
        // Arrange
        var name = "Food";
        var description = "Food and groceries";
        var icon = "🍔";
        var color = "#FF6B6B";

        // Act
        var category = Category.Create(name, description, icon, color);

        // Assert
        category.Should().NotBeNull();
        category.Name.Should().Be(name);
        category.Description.Should().Be(description);
        category.Icon.Should().Be(icon);
        category.Color.Should().Be(color);
        category.IsDefault.Should().BeTrue(); // No userId = default category
        category.UserId.Should().BeNull();
    }

    [Fact]
    public void Create_WithUserId_ShouldNotBeDefaultCategory()
    {
        // Arrange
        var userId = Guid.NewGuid();

        // Act
        var category = Category.Create("Custom", userId: userId);

        // Assert
        category.IsDefault.Should().BeFalse();
        category.UserId.Should().Be(userId);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Create_WithInvalidName_ShouldThrowArgumentException(string? name)
    {
        // Act
        var act = () => Category.Create(name!);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Category name is required*");
    }

    [Fact]
    public void Update_WithValidData_ShouldUpdateCategory()
    {
        // Arrange
        var category = Category.Create("Original", "Desc", "🔵", "#000000");
        var newName = "Updated";
        var newDescription = "Updated description";
        var newIcon = "🔴";
        var newColor = "#FF0000";

        // Act
        category.Update(newName, newDescription, newIcon, newColor);

        // Assert
        category.Name.Should().Be(newName);
        category.Description.Should().Be(newDescription);
        category.Icon.Should().Be(newIcon);
        category.Color.Should().Be(newColor);
        category.UpdatedAt.Should().NotBeNull();
    }
}
