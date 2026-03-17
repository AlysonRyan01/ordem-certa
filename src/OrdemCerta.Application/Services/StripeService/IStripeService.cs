using OrdemCerta.Shared;

namespace OrdemCerta.Application.Services.StripeService;

public interface IStripeService
{
    Task<Result<string>> CreateCheckoutSessionAsync(Guid companyId, string successUrl, string cancelUrl, CancellationToken cancellationToken);
    Task<Result<string>> CreateCustomerPortalSessionAsync(Guid companyId, string returnUrl, CancellationToken cancellationToken);
    Task<Result> HandleWebhookAsync(string payload, string stripeSignature, CancellationToken cancellationToken);
}
