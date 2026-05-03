using Application.Common;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Email;

public sealed class LoggingEmailSender(ILogger<LoggingEmailSender> logger) : IEmailSender
{
    public Task SendAsync(string toEmail, string subject, string htmlBody, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Email → {To}: {Subject}\n{Body}", toEmail, subject, htmlBody);
        return Task.CompletedTask;
    }
}
