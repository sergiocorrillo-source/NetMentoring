using System.Threading.Tasks;
using Ticketing.Services.DTOs;

namespace Ticketing.Services
{
    public interface IEmailProvider
    {
        Task<bool> SendEmailAsync(string to, string subject, string body);
    }
}
