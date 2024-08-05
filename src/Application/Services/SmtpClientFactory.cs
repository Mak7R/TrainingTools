using System.Net;
using System.Net.Mail;
using Application.Interfaces.Services;
using Application.Options;

namespace Application.Services;

public class SmtpClientFactory : ISmtpClientFactory
{
    public SmtpClient Create(EmailOptions options)
    {
        return new SmtpClient(options.SmtpHost)
        {
            Port = options.SmtpPort,
            Credentials = new NetworkCredential(options.Email, options.Password),
            EnableSsl = true
        };
    }
}