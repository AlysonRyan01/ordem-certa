using Microsoft.AspNetCore.Mvc;
using OrdemCerta.Application.Inputs.CompanyInputs;
using OrdemCerta.Application.Services.CompanyService;
using OrdemCerta.Domain.Companies.DTOs;

namespace OrdemCerta.Presentation.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CompanyController : ControllerBase
{
    private readonly ICompanyService _companyService;

    public CompanyController(ICompanyService companyService)
    {
        _companyService = companyService;
    }

    [HttpPost]
    [ProducesResponseType(typeof(CompanyOutput), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create(
        [FromBody] CreateCompanyInput input,
        CancellationToken cancellationToken)
    {
        var result = await _companyService.CreateAsync(input, cancellationToken);

        if (result.IsFailure)
            return BadRequest(new { errors = result.Errors });

        return CreatedAtAction(
            nameof(GetById),
            new { id = result.Value!.Id },
            result.Value);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(CompanyOutput), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(
        [FromRoute] Guid id,
        [FromBody] UpdateCompanyInput input,
        CancellationToken cancellationToken)
    {
        var result = await _companyService.UpdateAsync(id, input, cancellationToken);

        if (result.IsFailure)
            return BadRequest(new { errors = result.Errors });

        return Ok(result.Value);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(CompanyOutput), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        var result = await _companyService.GetByIdAsync(id, cancellationToken);

        if (result.IsFailure)
            return NotFound(new { errors = result.Errors });

        return Ok(result.Value);
    }
}
