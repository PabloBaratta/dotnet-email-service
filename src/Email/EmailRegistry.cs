namespace BeChallenge.Email
{
    public class EmailRegistry(IEnumerable<IEmailProvider> providers)
    {
        private readonly IList<IEmailProvider> _providers = [.. providers];

        public IEnumerable<IEmailProvider> Providers => _providers;
    }
}
