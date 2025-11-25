using Microsoft.Extensions.Configuration;

namespace BeChallenge.Tests
{
    public class ConfigFixture
    {
        public IConfiguration Configuration { get; }

        public ConfigFixture()
        {
            Configuration = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("appsettings.Test.json", optional: true, reloadOnChange: true)
                .Build();
        }
    }
}
