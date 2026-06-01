using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Ticketing.Services
{
    public class EmailProvider : IEmailProvider
    {
        private readonly ILogger<EmailProvider>? _logger;

        public EmailProvider(ILogger<EmailProvider>? logger = null)
        {
            _logger = logger;
        }

        public async Task<bool> SendEmailAsync(string to, string subject, string body)
        {
            // Simulate HTTP call to external provider
            _logger?.LogInformation("Simulating send email to {To}: {Subject}", to, subject);
            await Task.Delay(200); // simulate network
            return true;
        }
    }
}
