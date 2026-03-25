using Microsoft.AspNetCore.Mvc;
using OrdemCerta.Application.Inputs.CompanyInputs;

using OrdemCerta.Application.Services.AuthService;
using OrdemCerta.Application.Services.CompanyService;
using OrdemCerta.Domain.Companies.DTOs;

namespace OrdemCerta.Presentation.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly ICompanyService _companyService;

    public AuthController(IAuthService authService, ICompanyService companyService)
    {
        _authService = authService;
        _companyService = companyService;
    }

    [HttpPost("login")]
    [ProducesResponseType(typeof(TokenOutput), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Login(
        [FromBody] LoginInput input,
        CancellationToken cancellationToken)
    {
        var result = await _authService.LoginAsync(input, cancellationToken);

        if (result.IsFailure)
            return BadRequest(new { errors = result.Errors });

        return Ok(result.Value);
    }

    [HttpPost("refresh")]
    [ProducesResponseType(typeof(TokenOutput), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Refresh(
        [FromBody] RefreshTokenInput input,
        CancellationToken cancellationToken)
    {
        var result = await _authService.RefreshAsync(input.RefreshToken, cancellationToken);

        if (result.IsFailure)
            return BadRequest(new { errors = result.Errors });

        return Ok(result.Value);
    }

    [HttpPost("request-password-reset")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> RequestPasswordReset(
        [FromBody] RequestPasswordResetInput input,
        CancellationToken cancellationToken)
    {
        await _companyService.RequestPasswordResetAsync(input, cancellationToken);
        return NoContent();
    }

    [HttpPost("confirm-password-reset")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ConfirmPasswordReset(
        [FromBody] ConfirmPasswordResetInput input,
        CancellationToken cancellationToken)
    {
        var result = await _companyService.ConfirmPasswordResetAsync(input, cancellationToken);

        if (result.IsFailure)
            return BadRequest(new { errors = result.Errors });

        return NoContent();
    }
}
