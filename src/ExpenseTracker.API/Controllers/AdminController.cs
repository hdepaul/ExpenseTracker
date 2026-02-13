using ExpenseTracker.Application.Features.Admin;
using ExpenseTracker.Application.Features.Admin.Queries.GetUsers;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ExpenseTracker.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class AdminController : ControllerBase
{
    private readonly IMediator _mediator;

    public AdminController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("users")]
    [ProducesResponseType(typeof(List<AdminUserDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetUsers(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetUsersQuery(), cancellationToken);
        return Ok(result);
    }
}
