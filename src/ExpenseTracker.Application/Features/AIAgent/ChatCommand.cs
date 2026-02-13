using MediatR;

namespace ExpenseTracker.Application.Features.AIAgent;

public record ChatCommand(string Message, List<ChatMessageDto> History) : IRequest<ChatResponse>;

public record ChatMessageDto(string Role, string Content);
