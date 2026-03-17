using Microsoft.Extensions.Configuration;
using OrdemCerta.Domain.Companies.Enums;
using OrdemCerta.Infrastructure.DataContext.Uow;
using OrdemCerta.Infrastructure.Repositories.CompanyRepository;
using OrdemCerta.Shared;
using Stripe;
using Stripe.Checkout;

namespace OrdemCerta.Application.Services.StripeService;

public class StripeService : IStripeService
{
    private readonly ICompanyRepository _companyRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly string _priceId;
    private readonly string _webhookSecret;

    public StripeService(
        ICompanyRepository companyRepository,
        IUnitOfWork unitOfWork,
        IConfiguration configuration)
    {
        _companyRepository = companyRepository;
        _unitOfWork = unitOfWork;
        _priceId = configuration["Stripe:PriceId"]
            ?? throw new InvalidOperationException("Stripe:PriceId não configurado");
        _webhookSecret = configuration["Stripe:WebhookSecret"]
            ?? throw new InvalidOperationException("Stripe:WebhookSecret não configurado");

        StripeConfiguration.ApiKey = configuration["Stripe:SecretKey"]
            ?? throw new InvalidOperationException("Stripe:SecretKey não configurado");
    }

    public async Task<Result<string>> CreateCheckoutSessionAsync(
        Guid companyId,
        string successUrl,
        string cancelUrl,
        CancellationToken cancellationToken)
    {
        var companyResult = await _companyRepository.GetByIdAsync(companyId, cancellationToken);
        if (companyResult.IsFailure)
            return Result<string>.Failure(companyResult.Errors);

        var company = companyResult.Value!;
        var stripeCustomerId = company.StripeCustomerId;

        if (string.IsNullOrEmpty(stripeCustomerId))
        {
            var stripeCustomerService = new Stripe.CustomerService();
            var customer = await stripeCustomerService.CreateAsync(new CustomerCreateOptions
            {
                Email = company.Email,
                Name = company.Name.Value,
                Metadata = new Dictionary<string, string> { ["companyId"] = companyId.ToString() },
            }, cancellationToken: cancellationToken);

            stripeCustomerId = customer.Id;
            company.SetStripeCustomerId(stripeCustomerId);
            await _companyRepository.UpdateAsync(company, cancellationToken);
            await _unitOfWork.CommitAsync(cancellationToken);
        }

        var sessionService = new SessionService();
        var session = await sessionService.CreateAsync(new SessionCreateOptions
        {
            Customer = stripeCustomerId,
            Mode = "subscription",
            LineItems =
            [
                new SessionLineItemOptions
                {
                    Price = _priceId,
                    Quantity = 1,
                },
            ],
            SuccessUrl = successUrl,
            CancelUrl = cancelUrl,
            SubscriptionData = new SessionSubscriptionDataOptions
            {
                Metadata = new Dictionary<string, string> { ["companyId"] = companyId.ToString() },
            },
        }, cancellationToken: cancellationToken);

        return Result<string>.Success(session.Url);
    }

    public async Task<Result<string>> CreateCustomerPortalSessionAsync(
        Guid companyId,
        string returnUrl,
        CancellationToken cancellationToken)
    {
        var companyResult = await _companyRepository.GetByIdAsync(companyId, cancellationToken);
        if (companyResult.IsFailure)
            return Result<string>.Failure(companyResult.Errors);

        var company = companyResult.Value!;

        if (string.IsNullOrEmpty(company.StripeCustomerId))
            return Result<string>.Failure(["Empresa não possui assinatura ativa."]);

        var portalService = new Stripe.BillingPortal.SessionService();
        var session = await portalService.CreateAsync(new Stripe.BillingPortal.SessionCreateOptions
        {
            Customer = company.StripeCustomerId,
            ReturnUrl = returnUrl,
        }, cancellationToken: cancellationToken);

        return Result<string>.Success(session.Url);
    }

    public async Task<Result> HandleWebhookAsync(
        string payload,
        string stripeSignature,
        CancellationToken cancellationToken)
    {
        Event stripeEvent;

        try
        {
            stripeEvent = EventUtility.ConstructEvent(payload, stripeSignature, _webhookSecret);
        }
        catch
        {
            return Result.Failure(["Assinatura do webhook inválida."]);
        }

        switch (stripeEvent.Type)
        {
            case EventTypes.CheckoutSessionCompleted:
            {
                var session = (Session)stripeEvent.Data.Object;
                if (session.Mode != "subscription") break;

                var companyResult = await _companyRepository.GetByStripeCustomerIdAsync(session.CustomerId, cancellationToken);
                if (companyResult.IsFailure) break;

                var company = companyResult.Value!;
                company.ActivateSubscription(session.SubscriptionId);
                await _companyRepository.UpdateAsync(company, cancellationToken);
                await _unitOfWork.CommitAsync(cancellationToken);
                break;
            }

            case EventTypes.InvoicePaymentSucceeded:
            {
                var invoice = (Invoice)stripeEvent.Data.Object;
                if (invoice.BillingReason != "subscription_cycle") break;

                var companyResult = await _companyRepository.GetByStripeCustomerIdAsync(invoice.CustomerId, cancellationToken);
                if (companyResult.IsFailure) break;

                var company = companyResult.Value!;
                if (company.Plan != PlanType.Paid)
                {
                    company.ActivateSubscription(company.StripeSubscriptionId ?? string.Empty);
                    await _companyRepository.UpdateAsync(company, cancellationToken);
                    await _unitOfWork.CommitAsync(cancellationToken);
                }
                break;
            }

            case EventTypes.CustomerSubscriptionDeleted:
            {
                var subscription = (Subscription)stripeEvent.Data.Object;

                var companyResult = await _companyRepository.GetByStripeCustomerIdAsync(subscription.CustomerId, cancellationToken);
                if (companyResult.IsFailure) break;

                var company = companyResult.Value!;
                company.CancelSubscription();
                await _companyRepository.UpdateAsync(company, cancellationToken);
                await _unitOfWork.CommitAsync(cancellationToken);
                break;
            }
        }

        return Result.Success();
    }
}
