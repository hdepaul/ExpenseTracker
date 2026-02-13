using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using ExpenseTracker.Application.Common.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace ExpenseTracker.Infrastructure.Services;

public class ClaudeAgentService : IClaudeAgentService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<ClaudeAgentService> _logger;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public ClaudeAgentService(
        HttpClient httpClient,
        IConfiguration configuration,
        ILogger<ClaudeAgentService> logger)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<ClaudeAgentResult> ProcessAsync(
        string message,
        List<ChatMessage> history,
        IEnumerable<CategoryInfo> categories,
        CancellationToken ct)
    {
        var messages = BuildMessages(history, message);
        return await CallClaudeAsync(messages, categories, ct);
    }

    public async Task<ClaudeAgentResult> SendToolResultAsync(
        List<ChatMessage> history,
        string toolUseId,
        string toolName,
        string toolInput,
        string toolResult,
        IEnumerable<CategoryInfo> categories,
        CancellationToken ct)
    {
        var messages = new List<object>();

        // Add history
        foreach (var msg in history)
        {
            messages.Add(new { role = msg.Role, content = msg.Content });
        }

        // Add assistant message with tool_use block
        messages.Add(new
        {
            role = "assistant",
            content = new object[]
            {
                new
                {
                    type = "tool_use",
                    id = toolUseId,
                    name = toolName,
                    input = JsonSerializer.Deserialize<JsonElement>(toolInput)
                }
            }
        });

        // Add tool result
        messages.Add(new
        {
            role = "user",
            content = new object[]
            {
                new
                {
                    type = "tool_result",
                    tool_use_id = toolUseId,
                    content = toolResult
                }
            }
        });

        return await CallClaudeAsync(messages, categories, ct);
    }

    private List<object> BuildMessages(List<ChatMessage> history, string newMessage)
    {
        var messages = new List<object>();

        foreach (var msg in history)
        {
            messages.Add(new { role = msg.Role, content = msg.Content });
        }

        messages.Add(new { role = "user", content = newMessage });
        return messages;
    }

    private async Task<ClaudeAgentResult> CallClaudeAsync(
        List<object> messages,
        IEnumerable<CategoryInfo> categories,
        CancellationToken ct)
    {
        var model = _configuration["Claude:Model"] ?? "claude-haiku-4-5-20251001";
        var maxTokens = _configuration.GetValue<int>("Claude:MaxTokens", 1024);

        var systemPrompt = BuildSystemPrompt(categories);
        var tools = BuildTools();

        var requestBody = new
        {
            model,
            max_tokens = maxTokens,
            system = systemPrompt,
            tools,
            messages
        };

        var json = JsonSerializer.Serialize(requestBody, JsonOptions);
        _logger.LogDebug("Claude API request: {Request}", json);

        var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
        var response = await _httpClient.PostAsync("https://api.anthropic.com/v1/messages", content, ct);

        var responseBody = await response.Content.ReadAsStringAsync(ct);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError("Claude API error {StatusCode}: {Body}", response.StatusCode, responseBody);
            throw new InvalidOperationException($"Claude API error: {response.StatusCode}");
        }

        _logger.LogDebug("Claude API response: {Response}", responseBody);
        return ParseResponse(responseBody);
    }

    private string BuildSystemPrompt(IEnumerable<CategoryInfo> categories)
    {
        var today = DateTime.UtcNow.ToString("yyyy-MM-dd");
        var categoryList = string.Join("\n", categories.Select(c => $"- {c.Id}: {c.Name}"));

        return $"""
            Sos un asistente de gastos. Tenés dos capacidades:
            1. Registrar gastos: el usuario te dice qué gastó y vos lo registrás usando la tool create_expense.
            2. Consultar gastos: el usuario pregunta cuánto gastó y vos consultás usando la tool query_expenses.

            Fecha de hoy: {today}

            Categorías disponibles:
            {categoryList}

            Reglas:
            - Si el usuario no dice la fecha, usá la fecha de hoy ({today})
            - Si no encontrás la categoría exacta, preguntá sugiriendo las más parecidas de la lista
            - Si falta el monto, preguntá
            - Respondé en el mismo idioma que el usuario
            - Sé breve y amigable en las confirmaciones
            - Usá la tool create_expense para crear gastos, nunca respondas solo con texto si tenés toda la info
            - El campo description debe ser corto y descriptivo (ej: "Nafta", "Almuerzo", "Netflix")
            - Para consultas de gastos, usá query_expenses. Cuando recibas los datos, hacé un resumen claro y amigable.
            - Para "esta semana" usá lunes a domingo de la semana actual
            - Para "este mes" usá el primer y último día del mes actual
            """;
    }

    private static object[] BuildTools()
    {
        return
        [
            new
            {
                name = "create_expense",
                description = "Creates a new expense record for the user",
                input_schema = new
                {
                    type = "object",
                    properties = new Dictionary<string, object>
                    {
                        ["amount"] = new { type = "number", description = "The expense amount (positive number)" },
                        ["description"] = new { type = "string", description = "Short description of the expense" },
                        ["date"] = new { type = "string", description = "Date in YYYY-MM-DD format" },
                        ["categoryId"] = new { type = "string", description = "UUID of the category from the available list" },
                        ["notes"] = new { type = "string", description = "Optional additional notes" }
                    },
                    required = new[] { "amount", "description", "date", "categoryId" }
                }
            },
            new
            {
                name = "query_expenses",
                description = "Queries the user's expenses for a date range, optionally filtered by category. Returns totals and breakdown by category.",
                input_schema = new
                {
                    type = "object",
                    properties = new Dictionary<string, object>
                    {
                        ["dateFrom"] = new { type = "string", description = "Start date in YYYY-MM-DD format" },
                        ["dateTo"] = new { type = "string", description = "End date in YYYY-MM-DD format" },
                        ["categoryId"] = new { type = "string", description = "Optional: UUID of category to filter by" }
                    },
                    required = new[] { "dateFrom", "dateTo" }
                }
            }
        ];
    }

    private static ClaudeAgentResult ParseResponse(string responseBody)
    {
        var doc = JsonDocument.Parse(responseBody);
        var root = doc.RootElement;

        var stopReason = root.GetProperty("stop_reason").GetString() ?? "end_turn";

        string? toolUseId = null;
        string? toolName = null;
        JsonElement? toolInput = null;
        string? textContent = null;

        if (root.TryGetProperty("content", out var contentArray))
        {
            foreach (var block in contentArray.EnumerateArray())
            {
                var blockType = block.GetProperty("type").GetString();

                if (blockType == "tool_use")
                {
                    toolUseId = block.GetProperty("id").GetString();
                    toolName = block.GetProperty("name").GetString();
                    toolInput = block.GetProperty("input").Clone();
                }
                else if (blockType == "text")
                {
                    var text = block.GetProperty("text").GetString();
                    if (!string.IsNullOrWhiteSpace(text))
                    {
                        textContent = textContent == null ? text : textContent + "\n" + text;
                    }
                }
            }
        }

        return new ClaudeAgentResult(stopReason, toolUseId, toolName, toolInput, textContent);
    }
}
