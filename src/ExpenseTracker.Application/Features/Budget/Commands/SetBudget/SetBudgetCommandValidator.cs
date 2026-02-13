using FluentValidation;

namespace ExpenseTracker.Application.Features.Budget.Commands.SetBudget;

public class SetBudgetCommandValidator : AbstractValidator<SetBudgetCommand>
{
    public SetBudgetCommandValidator()
    {
        RuleFor(x => x.Amount)
            .GreaterThan(0)
            .WithMessage("Amount must be greater than zero");
    }
}
