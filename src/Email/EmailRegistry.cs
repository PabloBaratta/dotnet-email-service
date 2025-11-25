namespace BeChallenge.Email
{
    public class EmailRegistry(IEnumerable<IEmailProvider> providers)
    {
        private readonly IEnumerable<IEmailProvider> _providers = providers;

        public IEmailProvider GetProvider(int index)
        {
            return _providers.ElementAt(index);
        }
    }
}
