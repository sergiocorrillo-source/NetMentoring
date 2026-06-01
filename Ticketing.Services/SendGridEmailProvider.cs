using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace Ticketing.Services
{
    public class SendGridEmailProvider : IEmailProvider
    {
        private readonly string? _apiKey;
        private readonly ILogger<SendGridEmailProvider>? _logger;

        public SendGridEmailProvider(IConfiguration configuration, ILogger<SendGridEmailProvider>? logger = null)
        {
            _apiKey = configuration["SendGrid:ApiKey"];
            _logger = logger;
        }

        public async Task<bool> SendEmailAsync(string to, string subject, string body)
        {
            if (string.IsNullOrEmpty(_apiKey))
            {
                _logger?.LogWarning("SendGrid API key not configured. Falling back to simulated provider.");
                await Task.Delay(20);
                return true;
            }

            var client = new SendGridClient(_apiKey);
            var msg = new SendGridMessage()
            {
                From = new EmailAddress("noreply@example.com", "Ticketing"),
                Subject = subject,
                PlainTextContent = body,
                HtmlContent = body
            };
            msg.AddTo(new EmailAddress(to));

            var response = await client.SendEmailAsync(msg);
            return response.IsSuccessStatusCode;
        }
    }
}
