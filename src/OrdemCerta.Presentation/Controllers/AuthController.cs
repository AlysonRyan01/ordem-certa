using Microsoft.AspNetCore.Mvc;
using OrdemCerta.Application.Inputs.CompanyInputs;
using OrdemCerta.Application.Services.AuthService;
using OrdemCerta.Domain.Companies.DTOs;

namespace OrdemCerta.Presentation.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
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
}
