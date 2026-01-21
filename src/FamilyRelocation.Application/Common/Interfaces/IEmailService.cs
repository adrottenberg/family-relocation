namespace FamilyRelocation.Application.Common.Interfaces;

public interface IEmailService
{
    Task SendEmailAsync(string to, string subject, string htmlBody, CancellationToken ct = default);
    Task SendTemplatedEmailAsync(string to, string templateName, Dictionary<string, string> data, CancellationToken ct = default);
}
