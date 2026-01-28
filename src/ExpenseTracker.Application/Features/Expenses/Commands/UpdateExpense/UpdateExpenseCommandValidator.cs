using FluentValidation;

namespace ExpenseTracker.Application.Features.Expenses.Commands.UpdateExpense;

public class UpdateExpenseCommandValidator : AbstractValidator<UpdateExpenseCommand>
{
    public UpdateExpenseCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("Expense Id is required");

        RuleFor(x => x.Amount)
            .GreaterThan(0)
            .WithMessage("Amount must be greater than zero");

        RuleFor(x => x.Description)
            .NotEmpty()
            .WithMessage("Description is required")
            .MaximumLength(200)
            .WithMessage("Description must not exceed 200 characters");

        RuleFor(x => x.Date)
            .NotEmpty()
            .WithMessage("Date is required")
            .LessThanOrEqualTo(DateTime.UtcNow.AddDays(1))
            .WithMessage("Date cannot be in the future");

        RuleFor(x => x.CategoryId)
            .NotEmpty()
            .WithMessage("Category is required");

        RuleFor(x => x.Notes)
            .MaximumLength(500)
            .WithMessage("Notes must not exceed 500 characters");
    }
}
