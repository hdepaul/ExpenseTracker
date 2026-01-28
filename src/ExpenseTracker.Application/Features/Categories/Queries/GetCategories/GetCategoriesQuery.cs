using MediatR;

namespace ExpenseTracker.Application.Features.Categories.Queries.GetCategories;

public record GetCategoriesQuery : IRequest<IReadOnlyList<CategoryDto>>;
