using Microsoft.AspNetCore.Mvc;
using OrdemCerta.Application.Inputs.CustomerInputs;
using OrdemCerta.Application.Services.CustomerService;
using OrdemCerta.Domain.Customers.DTOs;
using OrdemCerta.Shared;

namespace OrdemCerta.Presentation.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CustomerController : ControllerBase
{
    private readonly ICustomerService _customerService;

    public CustomerController(ICustomerService customerService)
    {
        _customerService = customerService;
    }
    
    [HttpPost]
    [ProducesResponseType(typeof(CustomerOutput), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create(
        [FromBody] CreateCustomerInput input,
        CancellationToken cancellationToken)
    {
        var result = await _customerService.CreateAsync(input, cancellationToken);

        if (result.IsFailure)
            return BadRequest(new { errors = result.Errors });

        return CreatedAtAction(
            nameof(GetById),
            new { id = result.Value!.Id },
            result.Value);
    }
    
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(CustomerOutput), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(
        [FromRoute] Guid id,
        [FromBody] UpdateCustomerInput input,
        CancellationToken cancellationToken)
    {
        var result = await _customerService.UpdateAsync(id, input, cancellationToken);

        if (result.IsFailure)
            return BadRequest(new { errors = result.Errors });

        return Ok(result.Value);
    }
    
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        var result = await _customerService.DeleteAsync(id, cancellationToken);

        if (result.IsFailure)
            return NotFound(new { errors = result.Errors });

        return NoContent();
    }
    
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(CustomerOutput), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        var result = await _customerService.GetByIdAsync(id, cancellationToken);

        if (result.IsFailure)
            return NotFound(new { errors = result.Errors });

        return Ok(result.Value);
    }
    
    [HttpGet]
    [ProducesResponseType(typeof(List<CustomerOutput>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetPaged(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var input = new GetPagedInput(page, pageSize);
        var result = await _customerService.GetPagedAsync(input, cancellationToken);

        if (result.IsFailure)
            return BadRequest(new { errors = result.Errors });

        return Ok(new
        {
            page,
            pageSize,
            data = result.Value
        });
    }
    
    [HttpGet("search")]
    [ProducesResponseType(typeof(List<CustomerOutput>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetByName(
        [FromQuery] string searchTerm,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var input = new GetPagedInput(page, pageSize);
        var result = await _customerService.GetByNameAsync(searchTerm, input, cancellationToken);

        if (result.IsFailure)
            return BadRequest(new { errors = result.Errors });

        return Ok(new
        {
            searchTerm,
            page,
            pageSize,
            data = result.Value
        });
    }
    
    [HttpPost("{id:guid}/phones")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AddPhone(
        [FromRoute] Guid id,
        [FromBody] AddPhoneInput input,
        CancellationToken cancellationToken)
    {
        var result = await _customerService.AddPhoneAsync(id, input, cancellationToken);

        if (result.IsFailure)
            return BadRequest(new { errors = result.Errors });

        return NoContent();
    }
    
    [HttpDelete("{id:guid}/phones")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RemovePhone(
        [FromRoute] Guid id,
        [FromBody] RemovePhoneInput input,
        CancellationToken cancellationToken)
    {
        var result = await _customerService.RemovePhoneAsync(id, input, cancellationToken);

        if (result.IsFailure)
            return BadRequest(new { errors = result.Errors });

        return NoContent();
    }
}