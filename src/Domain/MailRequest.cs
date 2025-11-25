namespace BeChallenge.Domain
{
    public record MailRequest(
            string From,
            string To,
            string? Subject,
            string Body
        );
}