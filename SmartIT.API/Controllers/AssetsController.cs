using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartIT.Application.Assets;

namespace SmartIT.API.Controllers;

[ApiController]
[Authorize]
[Route("api/assets")]
public sealed class AssetsController(ISender sender) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<AssetListItem>>> Get(CancellationToken cancellationToken) =>
        Ok(await sender.Send(new GetAssetsQuery(), cancellationToken));

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<AssetListItem>> Get(Guid id, CancellationToken cancellationToken)
    {
        var asset = await sender.Send(new GetAssetByIdQuery(id), cancellationToken);
        return asset is null ? NotFound() : Ok(asset);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<AssetListItem>> Post(CreateAssetCommand command, CancellationToken cancellationToken)
    {
        var asset = await sender.Send(command, cancellationToken);
        return CreatedAtAction(nameof(Get), new { id = asset.Id }, asset);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Put(Guid id, UpdateAssetCommand command, CancellationToken cancellationToken)
    {
        if (id != command.Id) return BadRequest("Route id and payload id do not match.");
        return await sender.Send(command, cancellationToken) ? NoContent() : NotFound();
    }
}
