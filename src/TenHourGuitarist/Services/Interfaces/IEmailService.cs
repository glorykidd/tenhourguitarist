namespace TenHourGuitarist.Services.Interfaces;

public interface IEmailService
{
    Task SendVerifyEmailAsync(string toEmail, string userName, string verifyUrl);
    Task SendResetPasswordAsync(string toEmail, string userName, string resetUrl);
    Task SendOrderConfirmationAsync(string toEmail, string userName, string packageName, decimal amount, string orderRefId);
    Task SendOrderCancellationAsync(string toEmail, string userName, string packageName, string orderRefId);
    Task SendContactNotificationAsync(string fromName, string fromEmail, string subject, string message);
}
