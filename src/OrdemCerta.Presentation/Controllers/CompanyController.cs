using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OrdemCerta.Application.Inputs.CompanyInputs;
using OrdemCerta.Application.Services.CompanyService;
using OrdemCerta.Domain.Companies.DTOs;
using OrdemCerta.Domain.Companies.Interfaces;

namespace OrdemCerta.Presentation.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CompanyController : ControllerBase
{
    private readonly ICompanyService _companyService;
    private readonly ICurrentCompany _currentCompany;

    public CompanyController(ICompanyService companyService, ICurrentCompany currentCompany)
    {
        _companyService = companyService;
        _currentCompany = currentCompany;
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

    [Authorize]
    [HttpGet("me")]
    [ProducesResponseType(typeof(CompanyOutput), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMe(CancellationToken cancellationToken)
    {
        var result = await _companyService.GetByIdAsync(_currentCompany.CompanyId, cancellationToken);

        if (result.IsFailure)
            return NotFound(new { errors = result.Errors });

        return Ok(result.Value);
    }

    [Authorize]
    [HttpPut("me")]
    [ProducesResponseType(typeof(CompanyOutput), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateMe(
        [FromBody] UpdateCompanyInput input,
        CancellationToken cancellationToken)
    {
        var result = await _companyService.UpdateAsync(_currentCompany.CompanyId, input, cancellationToken);

        if (result.IsFailure)
            return BadRequest(new { errors = result.Errors });

        return Ok(result.Value);
    }

    [Authorize]
    [HttpPost("me/request-password-change")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RequestPasswordChange(CancellationToken cancellationToken)
    {
        var result = await _companyService.RequestPasswordChangeAsync(_currentCompany.CompanyId, cancellationToken);

        if (result.IsFailure)
            return BadRequest(new { errors = result.Errors });

        return NoContent();
    }

    [Authorize]
    [HttpPost("me/confirm-password-change")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ConfirmPasswordChange(
        [FromBody] ConfirmPasswordChangeInput input,
        CancellationToken cancellationToken)
    {
        var result = await _companyService.ConfirmPasswordChangeAsync(_currentCompany.CompanyId, input, cancellationToken);

        if (result.IsFailure)
            return BadRequest(new { errors = result.Errors });

        return NoContent();
    }
}
