using System.Text.Json;
using ExpenseTracker.Application.Common.Interfaces;
using ExpenseTracker.Domain.Entities;
using ExpenseTracker.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace ExpenseTracker.Application.Features.AIAgent;

public class ChatCommandHandler : IRequestHandler<ChatCommand, ChatResponse>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly IClaudeAgentService _claudeAgent;
    private readonly IConfiguration _configuration;
    private readonly ILogger<ChatCommandHandler> _logger;

    public ChatCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService,
        IClaudeAgentService claudeAgent,
        IConfiguration configuration,
        ILogger<ChatCommandHandler> logger)
    {
        _context = context;
        _currentUserService = currentUserService;
        _claudeAgent = claudeAgent;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<ChatResponse> Handle(ChatCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId
            ?? throw new UnauthorizedException("User must be authenticated");

        // Rate limit check
        var dailyLimit = _configuration.GetValue<int>("Claude:DailyMessageLimit", 30);
        var today = DateTime.UtcNow.Date;

        var usageLog = await _context.AIUsageLogs
            .FirstOrDefaultAsync(l => l.UserId == userId && l.Date == today, cancellationToken);

        if (usageLog != null && usageLog.MessageCount >= dailyLimit)
        {
            return new ChatResponse("message",
                "You've reached your daily AI message limit. Try again tomorrow!", null);
        }

        // Load categories
        var categories = await _context.Categories
            .Where(c => c.UserId == null || c.UserId == userId)
            .Select(c => new CategoryInfo(c.Id, c.Name))
            .ToListAsync(cancellationToken);

        // Convert history
        var history = request.History
            .Select(h => new ChatMessage(h.Role, h.Content))
            .ToList();

        // Call Claude
        var result = await _claudeAgent.ProcessAsync(
            request.Message, history, categories, cancellationToken);

        // Track usage
        if (usageLog == null)
        {
            usageLog = AIUsageLog.Create(userId, today);
            _context.AIUsageLogs.Add(usageLog);
        }
        else
        {
            usageLog.Increment();
        }
        await _context.SaveChangesAsync(cancellationToken);

        // Handle tool use
        if (result.StopReason == "tool_use" && result.ToolName == "create_expense" && result.ToolInput.HasValue)
        {
            return await HandleCreateExpense(
                result, userId, history, request.Message, categories, cancellationToken);
        }

        // Text response (question/clarification)
        return new ChatResponse("message", result.TextContent ?? "Sorry, I couldn't understand that.", null);
    }

    private async Task<ChatResponse> HandleCreateExpense(
        ClaudeAgentResult result,
        Guid userId,
        List<ChatMessage> history,
        string userMessage,
        List<CategoryInfo> categories,
        CancellationToken cancellationToken)
    {
        var input = result.ToolInput!.Value;

        var amount = input.GetProperty("amount").GetDecimal();
        var description = input.GetProperty("description").GetString()!;
        var dateStr = input.GetProperty("date").GetString()!;
        var categoryIdStr = input.GetProperty("categoryId").GetString()!;
        var notes = input.TryGetProperty("notes", out var notesEl) ? notesEl.GetString() : null;

        if (!DateTime.TryParse(dateStr, out var date))
            date = DateTime.UtcNow;

        if (!Guid.TryParse(categoryIdStr, out var categoryId))
        {
            return new ChatResponse("message", "I couldn't determine the category. Could you try again?", null);
        }

        // Verify category exists
        var categoryExists = await _context.Categories
            .AnyAsync(c => c.Id == categoryId &&
                          (c.UserId == null || c.UserId == userId), cancellationToken);

        if (!categoryExists)
        {
            return new ChatResponse("message", "The category wasn't found. Could you try again?", null);
        }

        // Create expense
        var expense = Expense.Create(amount, description, date, userId, categoryId, notes);
        _context.Expenses.Add(expense);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("AI created expense {ExpenseId} for user {UserId}: {Amount} - {Description}",
            expense.Id, userId, amount, description);

        // Send tool result back to Claude for confirmation message
        var fullHistory = new List<ChatMessage>(history)
        {
            new("user", userMessage)
        };

        var toolResultJson = JsonSerializer.Serialize(new
        {
            success = true,
            expenseId = expense.Id.ToString(),
            amount,
            description,
            date = date.ToString("yyyy-MM-dd")
        });

        var confirmResult = await _claudeAgent.SendToolResultAsync(
            fullHistory, result.ToolUseId!, toolResultJson, categories, cancellationToken);

        var confirmMessage = confirmResult.TextContent
            ?? $"Done! Added ${amount} for {description}.";

        return new ChatResponse("expense_created", confirmMessage, expense.Id);
    }
}
