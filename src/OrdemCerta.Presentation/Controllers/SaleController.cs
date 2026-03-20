using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using OrdemCerta.Application.Inputs.SaleInputs;
using OrdemCerta.Application.Services.SaleService;
using OrdemCerta.Domain.Sales.DTOs;
using OrdemCerta.Domain.Sales.Enums;
using OrdemCerta.Shared;

namespace OrdemCerta.Presentation.Controllers;

[Authorize]
[ApiController]
[Route("api/sales")]
[EnableRateLimiting("per-company")]
public class SaleController : ControllerBase
{
    private readonly ISaleService _saleService;

    public SaleController(ISaleService saleService)
    {
        _saleService = saleService;
    }

    [HttpPost]
    [ProducesResponseType(typeof(SaleOutput), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create(
        [FromBody] CreateSaleInput input,
        CancellationToken cancellationToken)
    {
        var result = await _saleService.CreateAsync(input, cancellationToken);

        if (result.IsFailure)
            return BadRequest(new { errors = result.Errors });

        return CreatedAtAction(nameof(GetById), new { id = result.Value!.Id }, result.Value);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(SaleOutput), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        var result = await _saleService.GetByIdAsync(id, cancellationToken);

        if (result.IsFailure)
            return NotFound(new { errors = result.Errors });

        return Ok(result.Value);
    }

    [HttpGet]
    [ProducesResponseType(typeof(List<SaleOutput>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPaged(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var result = await _saleService.GetPagedAsync(new GetPagedInput(page, pageSize), cancellationToken);
        return Ok(result.Value);
    }

    [HttpGet("by-status")]
    [ProducesResponseType(typeof(List<SaleOutput>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByStatus(
        [FromQuery] SaleStatus status,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var result = await _saleService.GetByStatusAsync(status, new GetPagedInput(page, pageSize), cancellationToken);
        return Ok(result.Value);
    }

    [HttpGet("by-customer/{customerId:guid}")]
    [ProducesResponseType(typeof(List<SaleOutput>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByCustomer(
        [FromRoute] Guid customerId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var result = await _saleService.GetByCustomerAsync(customerId, new GetPagedInput(page, pageSize), cancellationToken);
        return Ok(result.Value);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(SaleOutput), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Update(
        [FromRoute] Guid id,
        [FromBody] UpdateSaleInput input,
        CancellationToken cancellationToken)
    {
        var result = await _saleService.UpdateAsync(id, input, cancellationToken);

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
        var result = await _saleService.DeleteAsync(id, cancellationToken);

        if (result.IsFailure)
            return BadRequest(new { errors = result.Errors });

        return NoContent();
    }

    [HttpPost("{id:guid}/items")]
    [ProducesResponseType(typeof(SaleOutput), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> AddItem(
        [FromRoute] Guid id,
        [FromBody] AddSaleItemInput input,
        CancellationToken cancellationToken)
    {
        var result = await _saleService.AddItemAsync(id, input, cancellationToken);

        if (result.IsFailure)
            return BadRequest(new { errors = result.Errors });

        return Ok(result.Value);
    }

    [HttpPut("{id:guid}/items/{itemId:guid}")]
    [ProducesResponseType(typeof(SaleOutput), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateItem(
        [FromRoute] Guid id,
        [FromRoute] Guid itemId,
        [FromBody] UpdateSaleItemInput input,
        CancellationToken cancellationToken)
    {
        var result = await _saleService.UpdateItemAsync(id, itemId, input, cancellationToken);

        if (result.IsFailure)
            return BadRequest(new { errors = result.Errors });

        return Ok(result.Value);
    }

    [HttpDelete("{id:guid}/items/{itemId:guid}")]
    [ProducesResponseType(typeof(SaleOutput), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RemoveItem(
        [FromRoute] Guid id,
        [FromRoute] Guid itemId,
        CancellationToken cancellationToken)
    {
        var result = await _saleService.RemoveItemAsync(id, itemId, cancellationToken);

        if (result.IsFailure)
            return BadRequest(new { errors = result.Errors });

        return Ok(result.Value);
    }

    [HttpPost("{id:guid}/complete")]
    [ProducesResponseType(typeof(SaleOutput), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Complete(
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        var result = await _saleService.CompleteAsync(id, cancellationToken);

        if (result.IsFailure)
            return BadRequest(new { errors = result.Errors });

        return Ok(result.Value);
    }

    [HttpPost("{id:guid}/cancel")]
    [ProducesResponseType(typeof(SaleOutput), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Cancel(
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        var result = await _saleService.CancelAsync(id, cancellationToken);

        if (result.IsFailure)
            return BadRequest(new { errors = result.Errors });

        return Ok(result.Value);
    }

    [HttpPost("{id:guid}/warranty")]
    [ProducesResponseType(typeof(SaleOutput), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SetWarranty(
        [FromRoute] Guid id,
        [FromBody] SetSaleWarrantyInput input,
        CancellationToken cancellationToken)
    {
        var result = await _saleService.SetWarrantyAsync(id, input, cancellationToken);

        if (result.IsFailure)
            return BadRequest(new { errors = result.Errors });

        return Ok(result.Value);
    }

    [HttpGet("{id:guid}/print/receipt")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> PrintReceipt(
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        var result = await _saleService.PrintReceiptAsync(id, cancellationToken);

        if (result.IsFailure)
            return BadRequest(new { errors = result.Errors });

        return File(result.Value!, "application/pdf", $"venda-{id}.pdf");
    }

    [HttpGet("{id:guid}/print/warranty")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> PrintWarranty(
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        var result = await _saleService.PrintWarrantyAsync(id, cancellationToken);

        if (result.IsFailure)
            return BadRequest(new { errors = result.Errors });

        return File(result.Value!, "application/pdf", $"garantia-venda-{id}.pdf");
    }
}
