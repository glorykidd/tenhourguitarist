using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Stripe;
using Stripe.Checkout;
using TenHourGuitarist.Data;
using TenHourGuitarist.Data.Entities;
using TenHourGuitarist.Services.Interfaces;

namespace TenHourGuitarist.Services;

public class StripeService(
    IConfiguration configuration,
    ApplicationDbContext db,
    ISubscriptionService subscriptionService,
    IOrderService orderService,
    IEmailService emailService,
    UserManager<ApplicationUser> userManager) : IStripeService
{
    private string SecretKey => configuration["Stripe:SecretKey"]
        ?? throw new InvalidOperationException("Stripe:SecretKey is not configured.");
    private string WebhookSecret => configuration["Stripe:WebhookSecret"]
        ?? throw new InvalidOperationException("Stripe:WebhookSecret is not configured.");

    public async Task<string> CreateCheckoutSessionAsync(string userId, int packageId, string successUrl, string cancelUrl)
    {
        StripeConfiguration.ApiKey = SecretKey;

        var package = await db.Packages.FindAsync(packageId)
            ?? throw new InvalidOperationException($"Package with ID {packageId} not found.");

        var user = await userManager.FindByIdAsync(userId)
            ?? throw new InvalidOperationException($"User with ID {userId} not found.");

        // Create a pending order in the database
        var order = new Order
        {
            UserId = userId,
            PackageId = packageId,
            Amount = package.Amount,
            PaymentStatus = PaymentStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };

        order = await orderService.CreateAsync(order);

        // Create Stripe Checkout Session
        var options = new SessionCreateOptions
        {
            Mode = "subscription",
            CustomerEmail = user.Email,
            LineItems =
            [
                new SessionLineItemOptions
                {
                    Price = package.StripePriceId,
                    Quantity = 1
                }
            ],
            SuccessUrl = successUrl + "?session_id={CHECKOUT_SESSION_ID}",
            CancelUrl = cancelUrl,
            Metadata = new Dictionary<string, string>
            {
                { "order_id", order.Id.ToString() },
                { "user_id", userId },
                { "package_id", packageId.ToString() }
            }
        };

        var service = new SessionService();
        var session = await service.CreateAsync(options);

        // Store the Stripe session ID on the order
        order.StripeSessionId = session.Id;
        await orderService.UpdateAsync(order);

        return session.Url!;
    }

    public async Task HandleWebhookAsync(string json, string signature)
    {
        StripeConfiguration.ApiKey = SecretKey;

        var stripeEvent = EventUtility.ConstructEvent(json, signature, WebhookSecret);

        switch (stripeEvent.Type)
        {
            case EventTypes.CheckoutSessionCompleted:
                await HandleCheckoutSessionCompletedAsync(stripeEvent);
                break;

            case EventTypes.CustomerSubscriptionUpdated:
                await HandleSubscriptionUpdatedAsync(stripeEvent);
                break;

            case EventTypes.CustomerSubscriptionDeleted:
                await HandleSubscriptionDeletedAsync(stripeEvent);
                break;

            case EventTypes.InvoicePaymentFailed:
                await HandleInvoicePaymentFailedAsync(stripeEvent);
                break;
        }
    }

    private async Task HandleCheckoutSessionCompletedAsync(Event stripeEvent)
    {
        var session = stripeEvent.Data.Object as Session;
        if (session is null) return;

        // Update the order
        var order = await orderService.GetByStripeSessionIdAsync(session.Id);
        if (order is null) return;

        order.PaymentStatus = PaymentStatus.Success;
        order.StripePaymentIntentId = session.PaymentIntentId;
        order.PaymentDate = DateTime.UtcNow;
        await orderService.UpdateAsync(order);

        // Create subscription record
        var package = await db.Packages.FindAsync(order.PackageId);
        if (package is null) return;

        var endDate = package.DurationUnit == DurationUnit.Year
            ? DateTime.UtcNow.AddYears(package.DurationCount)
            : DateTime.UtcNow.AddMonths(package.DurationCount);

        var subscription = new Data.Entities.Subscription
        {
            UserId = order.UserId,
            PackageId = order.PackageId,
            StripeSubscriptionId = session.SubscriptionId,
            Status = SubscriptionStatus.Active,
            StartDate = DateTime.UtcNow,
            EndDate = endDate
        };

        await subscriptionService.CreateAsync(subscription);

        // Assign Subscriber role
        var user = await userManager.FindByIdAsync(order.UserId);
        if (user is not null)
        {
            if (!await userManager.IsInRoleAsync(user, "Subscriber"))
            {
                await userManager.AddToRoleAsync(user, "Subscriber");
            }

            // Send confirmation email
            await emailService.SendOrderConfirmationAsync(
                user.Email!,
                user.DisplayName,
                package.Title,
                order.Amount,
                order.OrderRefId.ToString());
        }
    }

    private async Task HandleSubscriptionUpdatedAsync(Event stripeEvent)
    {
        var stripeSubscription = stripeEvent.Data.Object as Stripe.Subscription;
        if (stripeSubscription is null) return;

        var status = stripeSubscription.Status switch
        {
            "active" => SubscriptionStatus.Active,
            "past_due" => SubscriptionStatus.PastDue,
            "canceled" => SubscriptionStatus.Cancelled,
            "unpaid" => SubscriptionStatus.PastDue,
            _ => SubscriptionStatus.Active
        };

        DateTime? endDate = stripeSubscription.Items?.Data?.FirstOrDefault()?.CurrentPeriodEnd;

        await subscriptionService.UpdateByStripeIdAsync(stripeSubscription.Id, status, endDate);
    }

    private async Task HandleSubscriptionDeletedAsync(Event stripeEvent)
    {
        var stripeSubscription = stripeEvent.Data.Object as Stripe.Subscription;
        if (stripeSubscription is null) return;

        var periodEnd = stripeSubscription.Items?.Data?.FirstOrDefault()?.CurrentPeriodEnd;
        var isExpired = periodEnd.HasValue && periodEnd.Value <= DateTime.UtcNow;
        var status = isExpired ? SubscriptionStatus.Expired : SubscriptionStatus.Cancelled;

        await subscriptionService.UpdateByStripeIdAsync(
            stripeSubscription.Id,
            status,
            periodEnd);

        // Send cancellation email
        var subscription = await db.Subscriptions
            .FirstOrDefaultAsync(s => s.StripeSubscriptionId == stripeSubscription.Id);

        if (subscription is not null)
        {
            var user = await userManager.FindByIdAsync(subscription.UserId);
            var package = await db.Packages.FindAsync(subscription.PackageId);

            if (user is not null && package is not null)
            {
                await emailService.SendOrderCancellationAsync(
                    user.Email!,
                    user.DisplayName,
                    package.Title,
                    string.Empty);
            }
        }
    }

    private async Task HandleInvoicePaymentFailedAsync(Event stripeEvent)
    {
        var invoice = stripeEvent.Data.Object as Invoice;
        var subscriptionId = invoice?.Parent?.SubscriptionDetails?.SubscriptionId;
        if (subscriptionId is null) return;

        await subscriptionService.UpdateByStripeIdAsync(
            subscriptionId,
            SubscriptionStatus.PastDue);
    }
}
