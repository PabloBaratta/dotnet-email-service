using BeChallenge.Domain;
using BeChallenge.Email;

namespace BeChallenge.Services
{
    public class EmailService(EmailRegistry emailRegistry)
    {
        private readonly EmailRegistry registry = emailRegistry;

        public void SendEmail(MailRequest mailRequest)
        {
            IEmailProvider provider = registry.GetProvider(0);
            _ = provider.SendEmail(mailRequest);
        }
    }
}