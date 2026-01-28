namespace ExpenseTracker.Application.Features.Categories;

public record CategoryDto(
    Guid Id,
    string Name,
    string? Description,
    string? Icon,
    string? Color,
    bool IsDefault);
