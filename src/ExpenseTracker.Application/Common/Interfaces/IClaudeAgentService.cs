using System.Text.Json;

namespace ExpenseTracker.Application.Common.Interfaces;

public interface IClaudeAgentService
{
    Task<ClaudeAgentResult> ProcessAsync(
        string message,
        List<ChatMessage> history,
        IEnumerable<CategoryInfo> categories,
        CancellationToken ct);

    Task<ClaudeAgentResult> SendToolResultAsync(
        List<ChatMessage> history,
        string toolUseId,
        string toolName,
        string toolInput,
        string toolResult,
        IEnumerable<CategoryInfo> categories,
        CancellationToken ct);
}

public record CategoryInfo(Guid Id, string Name);
public record ChatMessage(string Role, string Content);

public record ClaudeAgentResult(
    string StopReason,
    string? ToolUseId,
    string? ToolName,
    JsonElement? ToolInput,
    string? TextContent);
