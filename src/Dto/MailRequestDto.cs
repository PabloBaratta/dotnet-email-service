namespace ConsoleApp1;

public record MailRequestDto(
    [property: Required, EmailAddress] string From,
    [property: Required, EmailAddress] string To,
    [property: Required] string Subject,
    [property: Required] string Body,
);
