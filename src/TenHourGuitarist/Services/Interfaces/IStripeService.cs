namespace TenHourGuitarist.Services.Interfaces;

public interface IStripeService
{
    Task<string> CreateCheckoutSessionAsync(string userId, int packageId, string successUrl, string cancelUrl);
    Task HandleWebhookAsync(string json, string signature);
}
