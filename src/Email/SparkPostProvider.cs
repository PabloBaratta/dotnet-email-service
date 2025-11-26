using BeChallenge.Domain;
using SparkPost;

namespace BeChallenge.Email
{
    public class SparkPostProvider(IConfiguration config) : IEmailProvider
    {
        private readonly Client _client = new(
            config["SparkPost:ApiKey"] ?? throw new ArgumentNullException("SparkPost:ApiKey")
        );

        public async Task SendEmail(MailRequest mailRequest, CancellationToken ct = default)
        {
            Transmission transmission = new();

            transmission.Content.From = new Address { Email = mailRequest.From };
            transmission.Content.Subject = mailRequest.Subject ?? string.Empty;
            transmission.Content.Text = mailRequest.Body;

            Recipient recipient = new() { Address = new Address { Email = mailRequest.To } };
            transmission.Recipients.Add(recipient);

            SendTransmissionResponse? response = await _client.Transmissions.Send(transmission);

            if (response == null || response.TotalAcceptedRecipients == 0)
            {
                string details = response == null ? "no response" : response.Content ?? "no response";
                throw new TransientEmailException($"SparkPost send failed: {details}");
            }
        }
    }
}