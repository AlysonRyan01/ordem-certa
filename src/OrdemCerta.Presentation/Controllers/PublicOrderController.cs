using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using OrdemCerta.Application.Services.ServiceOrderService;

namespace OrdemCerta.Presentation.Controllers;

[ApiController]
[Route("public/orders")]
[EnableRateLimiting("public")]
public class PublicOrderController : ControllerBase
{
    private readonly IServiceOrderService _serviceOrderService;

    public PublicOrderController(IServiceOrderService serviceOrderService)
    {
        _serviceOrderService = serviceOrderService;
    }

    [HttpGet("/api/public/orders/{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetById(
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        var result = await _serviceOrderService.GetPublicByIdAsync(id, cancellationToken);

        if (result.IsFailure)
            return BadRequest(new { errors = result.Errors });

        return Ok(result.Value);
    }

    [HttpGet("{id:guid}/approve")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Approve(
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        var result = await _serviceOrderService.ApproveBudgetFromLinkAsync(id, cancellationToken);

        if (result.IsFailure)
            return BadRequest(new { errors = result.Errors });

        return Ok(result.Value);
    }

    [HttpGet("{id:guid}/refuse")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Refuse(
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        var result = await _serviceOrderService.RefuseBudgetFromLinkAsync(id, cancellationToken);

        if (result.IsFailure)
            return BadRequest(new { errors = result.Errors });

        return Ok(result.Value);
    }
}
