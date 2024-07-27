using System.Net.Mail;
using Application.Options;

namespace Application.Interfaces.Services;

public interface ISmtpClientFactory
{
    SmtpClient Create(EmailOptions options);
}