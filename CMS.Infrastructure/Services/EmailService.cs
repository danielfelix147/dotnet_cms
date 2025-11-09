using CMS.Application.Services;
using MailKit.Net.Smtp;
using Microsoft.Extensions.Configuration;
using MimeKit;
using System.Text.RegularExpressions;

namespace CMS.Infrastructure.Services;

public class EmailService : IEmailService
{
    private readonly IConfiguration _configuration;
    private readonly string _smtpHost;
    private readonly int _smtpPort;

    public EmailService(IConfiguration configuration)
    {
        _configuration = configuration;
        
        // Validate SMTP Host - check both configuration keys
        var host = _configuration["Smtp:Host"] ?? _configuration["EmailSettings:SmtpHost"];
        if (string.IsNullOrEmpty(host))
        {
            throw new ArgumentNullException("SmtpHost", "SMTP Host configuration is required.");
        }
        _smtpHost = host;
        
        // Validate and parse SMTP Port
        var portString = _configuration["Smtp:Port"] ?? _configuration["EmailSettings:SmtpPort"] ?? "1025";
        if (!int.TryParse(portString, out _smtpPort))
        {
            throw new FormatException($"Invalid SMTP Port configuration: {portString}");
        }
    }

    public async Task SendPasswordResetEmailAsync(string toEmail, string resetToken, string resetUrl)
    {
        // Validate email parameter
        if (toEmail == null)
        {
            throw new ArgumentNullException(nameof(toEmail), "Email address cannot be null.");
        }
        
        if (string.IsNullOrWhiteSpace(toEmail))
        {
            throw new ArgumentException("Email address cannot be empty.", nameof(toEmail));
        }
        
        // Validate email format
        if (!IsValidEmail(toEmail))
        {
            throw new FormatException($"Invalid email format: {toEmail}");
        }
        
        // Validate resetUrl parameter
        if (resetUrl == null)
        {
            throw new ArgumentNullException(nameof(resetUrl), "Reset URL cannot be null.");
        }
        
        if (string.IsNullOrWhiteSpace(resetUrl))
        {
            throw new ArgumentException("Reset URL cannot be empty.", nameof(resetUrl));
        }
        
        var smtpHost = _smtpHost;
        var smtpPort = _smtpPort;
        var fromEmail = _configuration["EmailSettings:FromEmail"] ?? "noreply@cms.com";
        var fromName = _configuration["EmailSettings:FromName"] ?? "CMS Support";
        var enableSsl = bool.Parse(_configuration["Smtp:EnableSsl"] ?? _configuration["EmailSettings:EnableSsl"] ?? "false");
        
        var resetLink = $"{resetUrl}?token={Uri.EscapeDataString(resetToken)}";

        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(fromName, fromEmail));
        message.To.Add(new MailboxAddress(toEmail, toEmail));
        message.Subject = "Password Reset Request - DOTNET CMS";

        var bodyBuilder = new BodyBuilder
        {
            HtmlBody = $@"
                <!DOCTYPE html>
                <html>
                <head>
                    <style>
                        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
                        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
                        .header {{ background-color: #4CAF50; color: white; padding: 20px; text-align: center; }}
                        .content {{ padding: 20px; background-color: #f9f9f9; }}
                        .button {{ display: inline-block; padding: 12px 24px; background-color: #4CAF50; color: white; text-decoration: none; border-radius: 4px; margin: 20px 0; }}
                        .footer {{ padding: 20px; text-align: center; font-size: 12px; color: #666; }}
                        .token-box {{ background-color: #f0f0f0; padding: 10px; border-left: 4px solid #4CAF50; margin: 20px 0; font-family: monospace; word-break: break-all; }}
                    </style>
                </head>
                <body>
                    <div class='container'>
                        <div class='header'>
                            <h1>Password Reset Request</h1>
                        </div>
                        <div class='content'>
                            <p>Hello,</p>
                            <p>You have requested to reset your password for your DOTNET CMS account.</p>
                            <p>Click the button below to reset your password:</p>
                            <p style='text-align: center;'>
                                <a href='{resetLink}' class='button'>Reset Password</a>
                            </p>
                            <p>Or copy and paste this link into your browser:</p>
                            <div class='token-box'>
                                {resetLink}
                            </div>
                            <p><strong>This link will expire in 24 hours.</strong></p>
                            <p>If you didn't request this password reset, please ignore this email. Your password will remain unchanged.</p>
                        </div>
                        <div class='footer'>
                            <p>This is an automated message from DOTNET CMS. Please do not reply to this email.</p>
                            <p>&copy; 2025 DOTNET CMS. All rights reserved.</p>
                        </div>
                    </div>
                </body>
                </html>",
            TextBody = $@"
Password Reset Request

Hello,

You have requested to reset your password for your DOTNET CMS account.

Click the link below to reset your password:
{resetLink}

This link will expire in 24 hours.

If you didn't request this password reset, please ignore this email. Your password will remain unchanged.

---
This is an automated message from DOTNET CMS. Please do not reply to this email.
© 2025 DOTNET CMS. All rights reserved."
        };

        message.Body = bodyBuilder.ToMessageBody();

        using var client = new SmtpClient();
        try
        {
            await client.ConnectAsync(smtpHost, smtpPort, enableSsl);
            
            // MailHog doesn't require authentication, but check if credentials are provided
            var username = _configuration["EmailSettings:Username"];
            var password = _configuration["EmailSettings:Password"];
            if (!string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(password))
            {
                await client.AuthenticateAsync(username, password);
            }

            await client.SendAsync(message);
            await client.DisconnectAsync(true);

            Console.WriteLine($"✅ Password reset email sent successfully to {toEmail}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Failed to send email: {ex.Message}");
            throw;
        }
    }

    private static bool IsValidEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return false;

        try
        {
            // Use a regex pattern for basic email validation
            var emailPattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
            return Regex.IsMatch(email, emailPattern, RegexOptions.IgnoreCase);
        }
        catch
        {
            return false;
        }
    }
}
