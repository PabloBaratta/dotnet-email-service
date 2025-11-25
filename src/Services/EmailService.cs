using BeChallenge.Domain;
using BeChallenge.Email;
using BeChallenge.Email.Retry;

namespace BeChallenge.Services
{
    public class EmailService(EmailRegistry emailRegistry, IRetryPolicy retryPolicy)
    {
        private readonly EmailRegistry registry = emailRegistry;
        private readonly IRetryPolicy _retryPolicy = retryPolicy;

        public async Task SendEmail(MailRequest mailRequest, CancellationToken ct = default)
        {
            ArgumentNullException.ThrowIfNull(mailRequest);
            ArgumentNullException.ThrowIfNull(_retryPolicy);

            IEnumerable<IEmailProvider> providers = registry.Providers;
            using IEnumerator<IEmailProvider> enumerator = providers.GetEnumerator();
            if (!enumerator.MoveNext())
            {
                throw new InvalidOperationException("No email providers registered.");
            }

            List<Exception> exceptions = [];

            foreach (IEmailProvider provider in providers)
            {
                try
                {
                    await _retryPolicy.ExecuteAsync(() => provider.SendEmail(mailRequest, ct), ct);
                    return;
                }
                catch (OperationCanceledException)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    exceptions.Add(new InvalidOperationException($"Provider {provider.GetType().Name} failed.", ex));
                }
            }

            throw new AggregateException("Failed to send email with all providers.", exceptions);
        }
    }
}