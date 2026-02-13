using FluentValidation;

namespace ExpenseTracker.Application.Features.AIAgent;

public class ChatCommandValidator : AbstractValidator<ChatCommand>
{
    public ChatCommandValidator()
    {
        RuleFor(x => x.Message)
            .NotEmpty()
            .WithMessage("Message is required")
            .MaximumLength(500)
            .WithMessage("Message must not exceed 500 characters");

        RuleFor(x => x.History)
            .Must(h => h == null || h.Count <= 20)
            .WithMessage("History must not exceed 20 messages");
    }
}
