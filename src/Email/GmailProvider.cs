using BeChallenge.Domain;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace BeChallenge.Email
{

    public class GmailProvider(IConfiguration config) : IEmailProvider
    {
        private readonly string _host = config["Smtp:Host"] ?? throw new ArgumentNullException("Smtp:Host");
        private readonly int _port = int.TryParse(config["Smtp:Port"], out int p) ? p : 587;
        private readonly string _user = config["Smtp:User"] ?? throw new ArgumentNullException("Smtp:User");
        private readonly string _password = config["Smtp:Password"] ?? throw new ArgumentNullException("Smtp:Password");
        private readonly bool _ssl = !bool.TryParse(config["Smtp:Ssl"], out bool s) || s;


        public async Task SendEmail(MailRequest mailRequest, CancellationToken ct = default)
        {
            MimeMessage message = new();
            message.From.Add(MailboxAddress.Parse(mailRequest.From));
            message.To.Add(MailboxAddress.Parse(mailRequest.To));
            message.Subject = mailRequest.Subject ?? string.Empty;
            BodyBuilder body = new() { TextBody = mailRequest.Body };
            message.Body = body.ToMessageBody();

            SmtpClient smtp = new();
            SecureSocketOptions socketOption = _ssl ? SecureSocketOptions.SslOnConnect : SecureSocketOptions.StartTls;

            await smtp.ConnectAsync(_host, _port, socketOption, ct);
            if (!string.IsNullOrEmpty(_user) && !string.IsNullOrEmpty(_password))
            {
                await smtp.AuthenticateAsync(_user, _password, ct);
            }

            _ = await smtp.SendAsync(message, ct);
            await smtp.DisconnectAsync(true, ct);
        }
    }
}