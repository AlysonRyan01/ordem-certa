using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OrdemCerta.Application.Abstractions;
using OrdemCerta.Application.Services.StripeService;
using OrdemCerta.Domain.Companies.Interfaces;

namespace OrdemCerta.Presentation.Controllers;

[ApiController]
[Route("api/billing")]
public class BillingController : ControllerBase
{
    private readonly IStripeService _stripeService;
    private readonly ICurrentCompany _currentCompany;
    private readonly IConfiguration _configuration;

    public BillingController(
        IStripeService stripeService,
        ICurrentCompany currentCompany,
        IConfiguration configuration)
    {
        _stripeService = stripeService;
        _currentCompany = currentCompany;
        _configuration = configuration;
    }

    [Authorize]
    [HttpPost("checkout")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateCheckout(CancellationToken cancellationToken)
    {
        var frontendUrl = _configuration["App:BaseUrl"] ?? string.Empty;

        var result = await _stripeService.CreateCheckoutSessionAsync(
            _currentCompany.CompanyId,
            successUrl: $"{frontendUrl}/billing/success",
            cancelUrl: $"{frontendUrl}/billing/cancel",
            cancellationToken);

        if (result.IsFailure)
            return BadRequest(new { errors = result.Errors });

        return Ok(new { url = result.Value });
    }

    [Authorize]
    [HttpPost("portal")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreatePortalSession(CancellationToken cancellationToken)
    {
        var frontendUrl = _configuration["App:BaseUrl"] ?? string.Empty;

        var result = await _stripeService.CreateCustomerPortalSessionAsync(
            _currentCompany.CompanyId,
            returnUrl: $"{frontendUrl}/billing",
            cancellationToken);

        if (result.IsFailure)
            return BadRequest(new { errors = result.Errors });

        return Ok(new { url = result.Value });
    }

    [HttpPost("/api/webhooks/stripe")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Webhook(CancellationToken cancellationToken)
    {
        var payload = await new StreamReader(Request.Body).ReadToEndAsync(cancellationToken);
        var signature = Request.Headers["Stripe-Signature"].ToString();

        var result = await _stripeService.HandleWebhookAsync(payload, signature, cancellationToken);

        if (result.IsFailure)
            return BadRequest(new { errors = result.Errors });

        return Ok();
    }
}
