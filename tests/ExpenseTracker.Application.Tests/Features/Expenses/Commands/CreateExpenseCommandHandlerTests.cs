using System.Linq.Expressions;
using ExpenseTracker.Application.Common.Interfaces;
using ExpenseTracker.Application.Features.Expenses.Commands.CreateExpense;
using ExpenseTracker.Domain.Entities;
using ExpenseTracker.Domain.Exceptions;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Moq;

namespace ExpenseTracker.Application.Tests.Features.Expenses.Commands;

public class CreateExpenseCommandHandlerTests
{
    private readonly Mock<IApplicationDbContext> _contextMock;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly CreateExpenseCommandHandler _handler;

    public CreateExpenseCommandHandlerTests()
    {
        _contextMock = new Mock<IApplicationDbContext>();
        _currentUserServiceMock = new Mock<ICurrentUserService>();
        _handler = new CreateExpenseCommandHandler(_contextMock.Object, _currentUserServiceMock.Object);
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldCreateExpenseAndReturnId()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();

        _currentUserServiceMock.Setup(x => x.UserId).Returns(userId);

        // Mock Categories DbSet
        var category = Category.Create("Test Category", userId: null);
        typeof(Category).GetProperty("Id")!.SetValue(category, categoryId);

        var categories = new List<Category> { category }.AsQueryable();
        var categoriesDbSetMock = CreateDbSetMock(categories);
        _contextMock.Setup(x => x.Categories).Returns(categoriesDbSetMock.Object);

        // Mock Expenses DbSet
        var expenses = new List<Expense>().AsQueryable();
        var expensesDbSetMock = CreateDbSetMock(expenses);
        _contextMock.Setup(x => x.Expenses).Returns(expensesDbSetMock.Object);

        _contextMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var command = new CreateExpenseCommand(
            Amount: 100m,
            Description: "Test Expense",
            Date: DateTime.UtcNow,
            CategoryId: categoryId,
            Notes: "Test notes"
        );

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeEmpty();
        expensesDbSetMock.Verify(x => x.Add(It.IsAny<Expense>()), Times.Once);
        _contextMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithUnauthenticatedUser_ShouldThrowUnauthorizedException()
    {
        // Arrange
        _currentUserServiceMock.Setup(x => x.UserId).Returns((Guid?)null);

        var command = new CreateExpenseCommand(
            Amount: 100m,
            Description: "Test",
            Date: DateTime.UtcNow,
            CategoryId: Guid.NewGuid(),
            Notes: null
        );

        // Act
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedException>()
            .WithMessage("*User must be authenticated*");
    }

    [Fact]
    public async Task Handle_WithNonExistentCategory_ShouldThrowNotFoundException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var nonExistentCategoryId = Guid.NewGuid();

        _currentUserServiceMock.Setup(x => x.UserId).Returns(userId);

        // Mock empty Categories DbSet (category doesn't exist)
        var categories = new List<Category>().AsQueryable();
        var categoriesDbSetMock = CreateDbSetMock(categories);
        _contextMock.Setup(x => x.Categories).Returns(categoriesDbSetMock.Object);

        var command = new CreateExpenseCommand(
            Amount: 100m,
            Description: "Test",
            Date: DateTime.UtcNow,
            CategoryId: nonExistentCategoryId,
            Notes: null
        );

        // Act
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage($"*Category*{nonExistentCategoryId}*not found*");
    }

    [Fact]
    public async Task Handle_WithCategoryBelongingToAnotherUser_ShouldThrowNotFoundException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();

        _currentUserServiceMock.Setup(x => x.UserId).Returns(userId);

        // Category belongs to another user
        var category = Category.Create("Other User Category", userId: otherUserId);
        typeof(Category).GetProperty("Id")!.SetValue(category, categoryId);

        var categories = new List<Category> { category }.AsQueryable();
        var categoriesDbSetMock = CreateDbSetMock(categories);
        _contextMock.Setup(x => x.Categories).Returns(categoriesDbSetMock.Object);

        var command = new CreateExpenseCommand(
            Amount: 100m,
            Description: "Test",
            Date: DateTime.UtcNow,
            CategoryId: categoryId,
            Notes: null
        );

        // Act
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    private static Mock<DbSet<T>> CreateDbSetMock<T>(IQueryable<T> data) where T : class
    {
        var mockSet = new Mock<DbSet<T>>();

        mockSet.As<IAsyncEnumerable<T>>()
            .Setup(m => m.GetAsyncEnumerator(It.IsAny<CancellationToken>()))
            .Returns(new TestAsyncEnumerator<T>(data.GetEnumerator()));

        mockSet.As<IQueryable<T>>()
            .Setup(m => m.Provider)
            .Returns(new TestAsyncQueryProvider<T>(data.Provider));

        mockSet.As<IQueryable<T>>()
            .Setup(m => m.Expression)
            .Returns(data.Expression);

        mockSet.As<IQueryable<T>>()
            .Setup(m => m.ElementType)
            .Returns(data.ElementType);

        mockSet.As<IQueryable<T>>()
            .Setup(m => m.GetEnumerator())
            .Returns(data.GetEnumerator());

        return mockSet;
    }
}

// Helper classes for async DbSet mocking
internal class TestAsyncQueryProvider<TEntity> : IAsyncQueryProvider
{
    private readonly IQueryProvider _inner;

    internal TestAsyncQueryProvider(IQueryProvider inner)
    {
        _inner = inner;
    }

    public IQueryable CreateQuery(Expression expression)
    {
        return new TestAsyncEnumerable<TEntity>(expression);
    }

    public IQueryable<TElement> CreateQuery<TElement>(Expression expression)
    {
        return new TestAsyncEnumerable<TElement>(expression);
    }

    public object? Execute(Expression expression)
    {
        return _inner.Execute(expression);
    }

    public TResult Execute<TResult>(Expression expression)
    {
        return _inner.Execute<TResult>(expression);
    }

    public TResult ExecuteAsync<TResult>(Expression expression, CancellationToken cancellationToken = default)
    {
        var resultType = typeof(TResult).GetGenericArguments()[0];
        var executionResult = typeof(IQueryProvider)
            .GetMethod(nameof(IQueryProvider.Execute), 1, new[] { typeof(Expression) })!
            .MakeGenericMethod(resultType)
            .Invoke(_inner, new[] { expression });

        return (TResult)typeof(Task)
            .GetMethod(nameof(Task.FromResult))!
            .MakeGenericMethod(resultType)
            .Invoke(null, new[] { executionResult })!;
    }
}

internal class TestAsyncEnumerable<T> : EnumerableQuery<T>, IAsyncEnumerable<T>, IQueryable<T>
{
    public TestAsyncEnumerable(IEnumerable<T> enumerable) : base(enumerable) { }
    public TestAsyncEnumerable(Expression expression) : base(expression) { }

    public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default)
    {
        return new TestAsyncEnumerator<T>(this.AsEnumerable().GetEnumerator());
    }

    IQueryProvider IQueryable.Provider => new TestAsyncQueryProvider<T>(this);
}

internal class TestAsyncEnumerator<T> : IAsyncEnumerator<T>
{
    private readonly IEnumerator<T> _inner;

    public TestAsyncEnumerator(IEnumerator<T> inner)
    {
        _inner = inner;
    }

    public T Current => _inner.Current;

    public ValueTask DisposeAsync()
    {
        _inner.Dispose();
        return ValueTask.CompletedTask;
    }

    public ValueTask<bool> MoveNextAsync()
    {
        return ValueTask.FromResult(_inner.MoveNext());
    }
}
