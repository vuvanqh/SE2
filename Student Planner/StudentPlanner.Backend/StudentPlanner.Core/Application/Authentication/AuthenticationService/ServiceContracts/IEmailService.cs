using System.Threading.Tasks;

namespace StudentPlanner.Core.Application.Authentication;

public interface IEmailService
{
    Task SendPasswordResetEmailAsync(string email, string token);
}
