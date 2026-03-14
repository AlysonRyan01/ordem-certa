using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OrdemCerta.Application.Inputs.ServiceOrderInputs;
using OrdemCerta.Application.Services.ServiceOrderService;
using OrdemCerta.Domain.ServiceOrders.DTOs;
using OrdemCerta.Domain.ServiceOrders.Enums;
using OrdemCerta.Shared;

namespace OrdemCerta.Presentation.Controllers;

[Authorize]
[ApiController]
[Route("api/service-orders")]
public class ServiceOrderController : ControllerBase
{
    private readonly IServiceOrderService _serviceOrderService;

    public ServiceOrderController(IServiceOrderService serviceOrderService)
    {
        _serviceOrderService = serviceOrderService;
    }

    [HttpPost]
    [ProducesResponseType(typeof(ServiceOrderOutput), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create(
        [FromBody] CreateServiceOrderInput input,
        CancellationToken cancellationToken)
    {
        var result = await _serviceOrderService.CreateAsync(input, cancellationToken);

        if (result.IsFailure)
            return BadRequest(new { errors = result.Errors });

        return CreatedAtAction(nameof(GetById), new { id = result.Value!.Id }, result.Value);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ServiceOrderOutput), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        var result = await _serviceOrderService.GetByIdAsync(id, cancellationToken);

        if (result.IsFailure)
            return NotFound(new { errors = result.Errors });

        return Ok(result.Value);
    }

    [HttpGet]
    [ProducesResponseType(typeof(List<ServiceOrderOutput>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPaged(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var result = await _serviceOrderService.GetPagedAsync(new GetPagedInput(page, pageSize), cancellationToken);
        return Ok(result.Value);
    }

    [HttpGet("by-status/{status}")]
    [ProducesResponseType(typeof(List<ServiceOrderOutput>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByStatus(
        [FromRoute] ServiceOrderStatus status,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var result = await _serviceOrderService.GetByStatusAsync(status, new GetPagedInput(page, pageSize), cancellationToken);
        return Ok(result.Value);
    }

    [HttpGet("by-customer/{customerId:guid}")]
    [ProducesResponseType(typeof(List<ServiceOrderOutput>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByCustomer(
        [FromRoute] Guid customerId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var result = await _serviceOrderService.GetByCustomerAsync(customerId, new GetPagedInput(page, pageSize), cancellationToken);
        return Ok(result.Value);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ServiceOrderOutput), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Update(
        [FromRoute] Guid id,
        [FromBody] UpdateServiceOrderInput input,
        CancellationToken cancellationToken)
    {
        var result = await _serviceOrderService.UpdateAsync(id, input, cancellationToken);

        if (result.IsFailure)
            return BadRequest(new { errors = result.Errors });

        return Ok(result.Value);
    }

    [HttpPatch("{id:guid}/status")]
    [ProducesResponseType(typeof(ServiceOrderOutput), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ChangeStatus(
        [FromRoute] Guid id,
        [FromBody] ChangeStatusInput input,
        CancellationToken cancellationToken)
    {
        var result = await _serviceOrderService.ChangeStatusAsync(id, input, cancellationToken);

        if (result.IsFailure)
            return BadRequest(new { errors = result.Errors });

        return Ok(result.Value);
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Delete(
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        var result = await _serviceOrderService.DeleteAsync(id, cancellationToken);

        if (result.IsFailure)
            return BadRequest(new { errors = result.Errors });

        return NoContent();
    }
}
