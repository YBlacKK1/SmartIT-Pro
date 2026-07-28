using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartIT.Application.Tickets;

namespace SmartIT.API.Controllers;

[ApiController]
[Authorize]
[Route("api/tickets")]
public sealed class TicketsController(ISender sender) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<TicketListItem>>> Get(CancellationToken cancellationToken) =>
        Ok(await sender.Send(new GetTicketsQuery(), cancellationToken));

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<TicketListItem>> Get(Guid id, CancellationToken cancellationToken)
    {
        var ticket = await sender.Send(new GetTicketByIdQuery(id), cancellationToken);
        return ticket is null ? NotFound() : Ok(ticket);
    }

    [HttpPost]
    public async Task<ActionResult<TicketListItem>> Post(CreateTicketCommand command, CancellationToken cancellationToken)
    {
        var ticket = await sender.Send(command, cancellationToken);
        return CreatedAtAction(nameof(Get), new { id = ticket.Id }, ticket);
    }
}
