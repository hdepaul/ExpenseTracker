using ExpenseTracker.Application.Features.Budget;
using ExpenseTracker.Application.Features.Budget.Commands.DeleteBudget;
using ExpenseTracker.Application.Features.Budget.Commands.SetBudget;
using ExpenseTracker.Application.Features.Budget.Queries.GetBudget;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ExpenseTracker.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class BudgetController : ControllerBase
{
    private readonly IMediator _mediator;

    public BudgetController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    [ProducesResponseType(typeof(BudgetDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> GetBudget(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetBudgetQuery(), cancellationToken);
        return result is null ? NoContent() : Ok(result);
    }

    [HttpPut]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SetBudget(
        [FromBody] SetBudgetRequest request,
        CancellationToken cancellationToken)
    {
        await _mediator.Send(new SetBudgetCommand(request.Amount), cancellationToken);
        return NoContent();
    }

    [HttpDelete]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteBudget(CancellationToken cancellationToken)
    {
        await _mediator.Send(new DeleteBudgetCommand(), cancellationToken);
        return NoContent();
    }
}

public record SetBudgetRequest(decimal Amount);
