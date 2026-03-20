using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OrdemCerta.Application.Services.AdminService;
using OrdemCerta.Domain.Admin.DTOs;

namespace OrdemCerta.Presentation.Controllers;

[ApiController]
[Route("api/admin")]
public class AdminController : ControllerBase
{
    private readonly IAdminService _adminService;

    public AdminController(IAdminService adminService)
    {
        _adminService = adminService;
    }

    [HttpPost("login")]
    [ProducesResponseType(typeof(AdminTokenOutput), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Login(
        [FromBody] AdminLoginRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _adminService.LoginAsync(request.Email, request.Password, cancellationToken);

        if (result.IsFailure)
            return BadRequest(new { errors = result.Errors });

        return Ok(result.Value);
    }

    [HttpGet("stats")]
    [Authorize(Policy = "Admin")]
    [ProducesResponseType(typeof(AdminStatsOutput), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetStats(CancellationToken cancellationToken)
    {
        var result = await _adminService.GetStatsAsync(cancellationToken);

        if (result.IsFailure)
            return BadRequest(new { errors = result.Errors });

        return Ok(result.Value);
    }

    [HttpGet("companies")]
    [Authorize(Policy = "Admin")]
    [ProducesResponseType(typeof(List<AdminCompanyOutput>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCompanies(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var result = await _adminService.GetCompaniesAsync(page, pageSize, cancellationToken);

        if (result.IsFailure)
            return BadRequest(new { errors = result.Errors });

        return Ok(result.Value);
    }
}

public record AdminLoginRequest(string Email, string Password);
