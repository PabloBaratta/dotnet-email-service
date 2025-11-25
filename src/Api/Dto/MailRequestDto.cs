using System.ComponentModel.DataAnnotations;
using BeChallenge.Domain;
namespace BeChallenge.Api.Dto
{
    public record MailRequestDto(
        [property: Required, EmailAddress] string To,
        [property: Required] string Subject,
        [property: Required] string Body
    )
    {
        public MailRequest ToDomain(string from)
        {
            return new MailRequest(from, To, Subject, Body);
        }
    }

}
