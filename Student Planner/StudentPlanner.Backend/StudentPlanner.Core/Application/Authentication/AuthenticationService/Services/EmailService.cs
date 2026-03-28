using System;
using System.Collections.Generic;
using System.Text;

namespace StudentPlanner.Core.Application.Authentication;

public class EmailService : IEmailService // to do kasia
{
    public Task SendPasswordResetEmailAsync(string email, string token)
    {
        throw new NotImplementedException();
    }
}
