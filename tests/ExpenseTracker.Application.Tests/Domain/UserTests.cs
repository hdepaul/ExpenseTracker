using ExpenseTracker.Domain.Entities;
using FluentAssertions;

namespace ExpenseTracker.Application.Tests.Domain;

public class UserTests
{
    [Fact]
    public void Create_WithValidData_ShouldCreateUser()
    {
        // Arrange
        var email = "test@example.com";
        var passwordHash = "hashedpassword123";
        var firstName = "John";
        var lastName = "Doe";

        // Act
        var user = User.Create(email, passwordHash, firstName, lastName);

        // Assert
        user.Should().NotBeNull();
        user.Email.Should().Be(email);
        user.PasswordHash.Should().Be(passwordHash);
        user.FirstName.Should().Be(firstName);
        user.LastName.Should().Be(lastName);
        user.Id.Should().NotBeEmpty();
        user.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Theory]
    [InlineData("", "hash", "John", "Doe")]
    [InlineData(" ", "hash", "John", "Doe")]
    [InlineData(null, "hash", "John", "Doe")]
    public void Create_WithInvalidEmail_ShouldThrowArgumentException(
        string? email, string passwordHash, string firstName, string lastName)
    {
        // Act
        var act = () => User.Create(email!, passwordHash, firstName, lastName);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Email is required*");
    }

    [Theory]
    [InlineData("test@example.com", "", "John", "Doe")]
    [InlineData("test@example.com", " ", "John", "Doe")]
    [InlineData("test@example.com", null, "John", "Doe")]
    public void Create_WithInvalidPasswordHash_ShouldThrowArgumentException(
        string email, string? passwordHash, string firstName, string lastName)
    {
        // Act
        var act = () => User.Create(email, passwordHash!, firstName, lastName);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Password is required*");
    }
}
