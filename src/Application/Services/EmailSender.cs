using System.Net.Mail;
using Application.Interfaces.Services;
using Application.Options;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Options;

namespace Application.Services;

public class EmailSender : IEmailSender
{
    private readonly ConfirmationEmailOptions _confirmationEmailOptions;
    private readonly ISmtpClientFactory _smtpClientFactory;

    public EmailSender(ISmtpClientFactory smtpClientFactory, IOptions<ConfirmationEmailOptions> options)
    {
        _smtpClientFactory = smtpClientFactory;
        _confirmationEmailOptions = options.Value;
    }

    public async Task SendEmailAsync(string email, string subject, string htmlMessage)
    {
        using var smtpClient = _smtpClientFactory.Create(_confirmationEmailOptions);

        var mailMessage = new MailMessage
        {
            To = { email },
            From = new MailAddress(_confirmationEmailOptions.Email),
            Subject = subject,
            Body = htmlMessage,
            IsBodyHtml = true
        };

        await smtpClient.SendMailAsync(mailMessage);
    }
}