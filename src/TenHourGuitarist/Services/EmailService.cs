using MailKit.Net.Smtp;
using MimeKit;
using TenHourGuitarist.Services.Interfaces;

namespace TenHourGuitarist.Services;

public class EmailService(IConfiguration configuration, IWebHostEnvironment environment) : IEmailService
{
    private string SmtpHost => configuration["Email:Host"] ?? "localhost";
    private int SmtpPort => int.TryParse(configuration["Email:Port"], out var port) ? port : 587;
    private string SmtpUsername => configuration["Email:Username"] ?? string.Empty;
    private string SmtpPassword => configuration["Email:Password"] ?? string.Empty;
    private string FromAddress => configuration["Email:FromAddress"] ?? "noreply@tenhourguitarist.com";
    private string FromName => configuration["Email:FromName"] ?? "TenHourGuitarist";

    public async Task SendVerifyEmailAsync(string toEmail, string userName, string verifyUrl)
    {
        var body = await LoadTemplateAsync("VerifyEmail.html");
        body = body.Replace("{{UserName}}", userName);
        body = body.Replace("{{VerifyUrl}}", verifyUrl);

        await SendEmailAsync(toEmail, "Verify Your Email - TenHourGuitarist", body);
    }

    public async Task SendResetPasswordAsync(string toEmail, string userName, string resetUrl)
    {
        var body = await LoadTemplateAsync("ResetPassword.html");
        body = body.Replace("{{UserName}}", userName);
        body = body.Replace("{{ResetUrl}}", resetUrl);

        await SendEmailAsync(toEmail, "Reset Your Password - TenHourGuitarist", body);
    }

    public async Task SendOrderConfirmationAsync(string toEmail, string userName, string packageName, decimal amount, string orderRefId)
    {
        var body = await LoadTemplateAsync("OrderConfirmation.html");
        body = body.Replace("{{UserName}}", userName);
        body = body.Replace("{{PackageName}}", packageName);
        body = body.Replace("{{Amount}}", amount.ToString("C"));
        body = body.Replace("{{OrderRefId}}", orderRefId);

        await SendEmailAsync(toEmail, "Order Confirmation - TenHourGuitarist", body);
    }

    public async Task SendOrderCancellationAsync(string toEmail, string userName, string packageName, string orderRefId)
    {
        var body = await LoadTemplateAsync("OrderCancellation.html");
        body = body.Replace("{{UserName}}", userName);
        body = body.Replace("{{PackageName}}", packageName);
        body = body.Replace("{{OrderRefId}}", orderRefId);

        await SendEmailAsync(toEmail, "Subscription Cancelled - TenHourGuitarist", body);
    }

    public async Task SendContactNotificationAsync(string fromName, string fromEmail, string subject, string message)
    {
        var body = await LoadTemplateAsync("ContactNotification.html");
        body = body.Replace("{{FromName}}", fromName);
        body = body.Replace("{{FromEmail}}", fromEmail);
        body = body.Replace("{{Subject}}", subject);
        body = body.Replace("{{Message}}", message);

        await SendEmailAsync(FromAddress, $"Contact Form: {subject}", body);
    }

    private async Task<string> LoadTemplateAsync(string templateName)
    {
        var templatePath = Path.Combine(environment.ContentRootPath, "EmailTemplates", templateName);

        if (!File.Exists(templatePath))
        {
            return string.Empty;
        }

        return await File.ReadAllTextAsync(templatePath);
    }

    private async Task SendEmailAsync(string toEmail, string subject, string htmlBody)
    {
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(FromName, FromAddress));
        message.To.Add(MailboxAddress.Parse(toEmail));
        message.Subject = subject;

        var bodyBuilder = new BodyBuilder
        {
            HtmlBody = htmlBody
        };
        message.Body = bodyBuilder.ToMessageBody();

        using var client = new SmtpClient();
        await client.ConnectAsync(SmtpHost, SmtpPort, MailKit.Security.SecureSocketOptions.StartTls);

        if (!string.IsNullOrEmpty(SmtpUsername))
        {
            await client.AuthenticateAsync(SmtpUsername, SmtpPassword);
        }

        await client.SendAsync(message);
        await client.DisconnectAsync(true);
    }
}
