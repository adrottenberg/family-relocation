using Amazon.SimpleEmail;
using Amazon.SimpleEmail.Model;
using FamilyRelocation.Application.Common.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace FamilyRelocation.Infrastructure.Services;

public class SesEmailService : IEmailService
{
    private readonly IAmazonSimpleEmailService _ses;
    private readonly IConfiguration _configuration;
    private readonly ILogger<SesEmailService> _logger;
    private readonly string _fromAddress;

    public SesEmailService(
        IAmazonSimpleEmailService ses,
        IConfiguration configuration,
        ILogger<SesEmailService> logger)
    {
        _ses = ses;
        _configuration = configuration;
        _logger = logger;
        _fromAddress = configuration["AWS:SES:FromAddress"] ?? "noreply@example.com";
    }

    public async Task SendEmailAsync(string to, string subject, string htmlBody, CancellationToken ct = default)
    {
        try
        {
            var request = new SendEmailRequest
            {
                Source = _fromAddress,
                Destination = new Destination { ToAddresses = new List<string> { to } },
                Message = new Message
                {
                    Subject = new Content(subject),
                    Body = new Body
                    {
                        Html = new Content(htmlBody)
                    }
                }
            };

            await _ses.SendEmailAsync(request, ct);
            _logger.LogInformation("Email sent to {To} with subject: {Subject}", to, subject);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email to {To}", to);
            // Don't throw - email failures shouldn't break the main flow
        }
    }

    public async Task SendTemplatedEmailAsync(string to, string templateName, Dictionary<string, string> data, CancellationToken ct = default)
    {
        var (subject, body) = GetTemplate(templateName, data);
        await SendEmailAsync(to, subject, body, ct);
    }

    private (string Subject, string Body) GetTemplate(string templateName, Dictionary<string, string> data)
    {
        return templateName switch
        {
            "ApplicationReceived" => (
                "Application Received - Vaad HaYishuv",
                GetApplicationReceivedTemplate(data)
            ),
            "BoardDecisionApproved" => (
                "Great News! Your Application Has Been Approved",
                GetBoardDecisionApprovedTemplate(data)
            ),
            "BoardDecisionRejected" => (
                "Update on Your Application",
                GetBoardDecisionRejectedTemplate(data)
            ),
            "BoardDecisionDeferred" => (
                "Update on Your Application",
                GetBoardDecisionDeferredTemplate(data)
            ),
            "StageChanged" => (
                $"Status Update: {data.GetValueOrDefault("NewStage", "Update")}",
                GetStageChangedTemplate(data)
            ),
            _ => ("Notification", $"<p>{string.Join("<br>", data.Select(kv => $"{kv.Key}: {kv.Value}"))}</p>")
        };
    }

    private string GetApplicationReceivedTemplate(Dictionary<string, string> data)
    {
        var husbandFirstName = data.GetValueOrDefault("HusbandFirstName", "");
        var husbandLastName = data.GetValueOrDefault("HusbandLastName", "");
        return $@"
<!DOCTYPE html>
<html>
<head><style>body {{ font-family: Arial, sans-serif; line-height: 1.6; }}</style></head>
<body>
<h2>Thank You for Your Application</h2>
<p>Dear {husbandFirstName} {husbandLastName},</p>
<p>We have received your application to join our community in Union County. Thank you for your interest!</p>
<p><strong>What happens next:</strong></p>
<ul>
<li>Our board will review your application</li>
<li>You will be notified once a decision has been made</li>
<li>If approved, we will contact you about the next steps</li>
</ul>
<p>If you have any questions, please don't hesitate to contact us.</p>
<p>Best regards,<br>Vaad HaYishuv</p>
</body>
</html>";
    }

    private string GetBoardDecisionApprovedTemplate(Dictionary<string, string> data)
    {
        var husbandFirstName = data.GetValueOrDefault("HusbandFirstName", "");
        var husbandLastName = data.GetValueOrDefault("HusbandLastName", "");
        return $@"
<!DOCTYPE html>
<html>
<head><style>body {{ font-family: Arial, sans-serif; line-height: 1.6; }}</style></head>
<body>
<h2>Congratulations!</h2>
<p>Dear {husbandFirstName} {husbandLastName},</p>
<p>We are pleased to inform you that your application has been <strong>approved</strong> by our board!</p>
<p><strong>Next Steps:</strong></p>
<ul>
<li>Sign the required agreements (broker agreement, community takanos)</li>
<li>Share your housing preferences with us</li>
<li>We will begin matching you with available properties</li>
</ul>
<p>Welcome to our community! We look forward to helping you find your new home.</p>
<p>Best regards,<br>Vaad HaYishuv</p>
</body>
</html>";
    }

    private string GetBoardDecisionRejectedTemplate(Dictionary<string, string> data)
    {
        var husbandFirstName = data.GetValueOrDefault("HusbandFirstName", "");
        var husbandLastName = data.GetValueOrDefault("HusbandLastName", "");
        return $@"
<!DOCTYPE html>
<html>
<head><style>body {{ font-family: Arial, sans-serif; line-height: 1.6; }}</style></head>
<body>
<h2>Application Update</h2>
<p>Dear {husbandFirstName} {husbandLastName},</p>
<p>Thank you for your interest in joining our community. After careful consideration, the board has decided not to proceed with your application at this time.</p>
<p>If you have any questions about this decision, please feel free to contact us.</p>
<p>We wish you the best in your search for a new home.</p>
<p>Best regards,<br>Vaad HaYishuv</p>
</body>
</html>";
    }

    private string GetBoardDecisionDeferredTemplate(Dictionary<string, string> data)
    {
        var husbandFirstName = data.GetValueOrDefault("HusbandFirstName", "");
        var husbandLastName = data.GetValueOrDefault("HusbandLastName", "");
        return $@"
<!DOCTYPE html>
<html>
<head><style>body {{ font-family: Arial, sans-serif; line-height: 1.6; }}</style></head>
<body>
<h2>Application Update</h2>
<p>Dear {husbandFirstName} {husbandLastName},</p>
<p>Thank you for your patience. The board has reviewed your application and has decided to defer the decision for now.</p>
<p>We will be in touch with you soon regarding next steps or any additional information we may need.</p>
<p>Best regards,<br>Vaad HaYishuv</p>
</body>
</html>";
    }

    private string GetStageChangedTemplate(Dictionary<string, string> data)
    {
        var husbandFirstName = data.GetValueOrDefault("HusbandFirstName", "");
        var husbandLastName = data.GetValueOrDefault("HusbandLastName", "");
        var newStage = data.GetValueOrDefault("NewStage", "");
        var message = newStage switch
        {
            "Searching" => "Great news! You are now in the active house hunting phase. We will be sending you property matches soon.",
            "UnderContract" => "Congratulations! Your property is now under contract. We will guide you through the closing process.",
            "Closed" => "Congratulations on closing! Welcome to Union County!",
            _ => $"Your status has been updated to: {newStage}"
        };

        return $@"
<!DOCTYPE html>
<html>
<head><style>body {{ font-family: Arial, sans-serif; line-height: 1.6; }}</style></head>
<body>
<h2>Status Update</h2>
<p>Dear {husbandFirstName} {husbandLastName},</p>
<p>{message}</p>
<p>If you have any questions, please don't hesitate to contact us.</p>
<p>Best regards,<br>Vaad HaYishuv</p>
</body>
</html>";
    }
}
