using BeChallenge.Domain;

namespace BeChallenge.Email
{
    public interface IEmailProvider
    {
        Task SendEmail(MailRequest mailRequest, CancellationToken ct = default);
    }
}
