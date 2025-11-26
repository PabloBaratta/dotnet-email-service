using System.Reflection;
using BeChallenge.Email;
using BeChallenge.Domain;
using Microsoft.Extensions.Configuration;

namespace BeChallenge.Tests.Email
{
    public class SparkPostProviderTests(ConfigFixture fixture) : IClassFixture<ConfigFixture>
    {
        private readonly IConfiguration _config = fixture.Configuration;

        private static IConfiguration BuildConfig(Dictionary<string, string> values)
        {
            IEnumerable<KeyValuePair<string, string?>> converted = values.Select(kv => new KeyValuePair<string, string?>(kv.Key, kv.Value));
            return new ConfigurationBuilder().AddInMemoryCollection(converted).Build();
        }

        [Fact]
        public void ConstructorThrowsWhenApiKeyMissing()
        {
            IConfiguration cfg = BuildConfig(new Dictionary<string, string>());

            ArgumentNullException ex = Assert.Throws<ArgumentNullException>(() => new SparkPostProvider(cfg));
            Assert.Equal("SparkPost:ApiKey", ex.ParamName);
        }

        [Fact]
        public async Task SendsEmailWithConfiguredProvider()
        {
            if (string.IsNullOrEmpty(_config["SparkPost:ApiKey"]) ||
                string.IsNullOrEmpty(_config["Mail:From"]) ||
                string.IsNullOrEmpty(_config["Smtp:To"]))
            {
                return;
            }

            SparkPostProvider provider = new(_config);

            MailRequest mail = new(
                _config["Mail:From"]!,
                _config["Smtp:To"]!,
                "BeChallenge Integration Test - SparkPost",
                $"Integration test message at {DateTime.UtcNow:O}"
            );

            using CancellationTokenSource cts = new(TimeSpan.FromSeconds(60));
            await provider.SendEmail(mail, cts.Token);
        }
    }
}
