using MailKit.Net.Smtp;
using MimeKit;

namespace DuoCareAPI.Services;

public class MailtrapEmailService
{
    private readonly IConfiguration _config;

    public MailtrapEmailService(IConfiguration config)
    {
        _config = config;
    }

    public async Task SendEmailAsync(string toEmail, string subject, string htmlMessage)
    {
        var email = new MimeMessage();

        email.From.Add(new MailboxAddress(
            _config["Mailtrap:FromName"],
            _config["Mailtrap:FromEmail"]
        ));

        email.To.Add(new MailboxAddress("", toEmail));
        email.Subject = subject;

        email.Body = new TextPart("html")
        {
            Text = htmlMessage
        };

        using var smtp = new SmtpClient();

        // Solucion al error de certificado en Windows?espero
        smtp.ServerCertificateValidationCallback = (s, c, h, e) => true;

        await smtp.ConnectAsync(
            _config["Mailtrap:Host"],
            int.Parse(_config["Mailtrap:Port"]),
            MailKit.Security.SecureSocketOptions.StartTls
        );

        await smtp.AuthenticateAsync(
            _config["Mailtrap:Username"],
            _config["Mailtrap:Password"]
        );

        await smtp.SendAsync(email);
        await smtp.DisconnectAsync(true);
    }
}
