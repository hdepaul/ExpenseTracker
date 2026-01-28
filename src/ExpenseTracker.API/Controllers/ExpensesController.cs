using ExpenseTracker.Application.Features.Expenses;
using ExpenseTracker.Application.Features.Expenses.Commands.CreateExpense;
using ExpenseTracker.Application.Features.Expenses.Commands.DeleteExpense;
using ExpenseTracker.Application.Features.Expenses.Commands.UpdateExpense;
using ExpenseTracker.Application.Features.Expenses.Queries.GetExpenseById;
using ExpenseTracker.Application.Features.Expenses.Queries.GetExpenses;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ExpenseTracker.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ExpensesController : ControllerBase
{
    private readonly IMediator _mediator;

    public ExpensesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Get all expenses with optional filtering and pagination
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ExpenseDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetExpenses(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] Guid? categoryId = null,
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null,
        CancellationToken cancellationToken = default)
    {
        var query = new GetExpensesQuery(pageNumber, pageSize, categoryId, fromDate, toDate);
        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Get a specific expense by ID
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ExpenseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetExpense(Guid id, CancellationToken cancellationToken)
    {
        var query = new GetExpenseByIdQuery(id);
        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Create a new expense
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateExpense(
        [FromBody] CreateExpenseRequest request,
        CancellationToken cancellationToken)
    {
        var command = new CreateExpenseCommand(
            request.Amount,
            request.Description,
            request.Date,
            request.CategoryId,
            request.Notes);

        var id = await _mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetExpense), new { id }, id);
    }

    /// <summary>
    /// Update an existing expense
    /// </summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateExpense(
        Guid id,
        [FromBody] UpdateExpenseRequest request,
        CancellationToken cancellationToken)
    {
        var command = new UpdateExpenseCommand(
            id,
            request.Amount,
            request.Description,
            request.Date,
            request.CategoryId,
            request.Notes);

        await _mediator.Send(command, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Delete an expense
    /// </summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteExpense(Guid id, CancellationToken cancellationToken)
    {
        var command = new DeleteExpenseCommand(id);
        await _mediator.Send(command, cancellationToken);
        return NoContent();
    }
}

public record CreateExpenseRequest(
    decimal Amount,
    string Description,
    DateTime Date,
    Guid CategoryId,
    string? Notes);

public record UpdateExpenseRequest(
    decimal Amount,
    string Description,
    DateTime Date,
    Guid CategoryId,
    string? Notes);
