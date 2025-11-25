using System.Reflection;
using BeChallenge.Email;
using BeChallenge.Domain;
using Microsoft.Extensions.Configuration;
 

namespace BeChallenge.Tests.Email
{
    public class GmailProviderTests(ConfigFixture fixture) : IClassFixture<ConfigFixture>
    {
        private readonly IConfiguration _config = fixture.Configuration;
        private static IConfiguration BuildConfig(Dictionary<string, string> values)
        {
            IEnumerable<KeyValuePair<string, string?>> converted = values.Select(kv => new KeyValuePair<string, string?>(kv.Key, kv.Value));
            return new ConfigurationBuilder().AddInMemoryCollection(converted).Build();
        }

        private static object GetPrivateField(object instance, string fieldName)
        {
            return instance.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(instance)!;
        }

        [Fact]
        public void ConstructorThrowsWhenHostMissing()
        {
            IConfiguration cfg = BuildConfig(new Dictionary<string, string>
            {
                {"Smtp:User", "user@example.com"},
                {"Smtp:Password", "secret"}
            });

            ArgumentNullException ex = Assert.Throws<ArgumentNullException>(() => new GmailProvider(cfg));
            Assert.Equal("Smtp:Host", ex.ParamName);
        }

        [Fact]
        public void ConstructorThrowsWhenUserMissing()
        {
            IConfiguration cfg = BuildConfig(new Dictionary<string, string>
            {
                {"Smtp:Host", "smtp.example.com"},
                {"Smtp:Password", "secret"}
            });

            ArgumentNullException ex = Assert.Throws<ArgumentNullException>(() => new GmailProvider(cfg));
            Assert.Equal("Smtp:User", ex.ParamName);
        }

        [Fact]
        public void ConstructorThrowsWhenPasswordMissing()
        {
            IConfiguration cfg = BuildConfig(new Dictionary<string, string>
            {
                {"Smtp:Host", "smtp.example.com"},
                {"Smtp:User", "user@example.com"}
            });

            ArgumentNullException ex = Assert.Throws<ArgumentNullException>(() => new GmailProvider(cfg));
            Assert.Equal("Smtp:Password", ex.ParamName);
        }

        [Fact]
        public void ReadsHostUserPasswordAndDefaultsPortAndSslCorrectly()
        {
            IConfiguration cfg = BuildConfig(new Dictionary<string, string>
            {
                {"Smtp:Host", "smtp.example.com"},
                {"Smtp:User", "user@example.com"},
                {"Smtp:Password", "secret"}
            });

            GmailProvider provider = new(cfg);

            string host = (string)GetPrivateField(provider, "_host");
            string user = (string)GetPrivateField(provider, "_user");
            string password = (string)GetPrivateField(provider, "_password");
            int port = (int)GetPrivateField(provider, "_port");
            bool ssl = (bool)GetPrivateField(provider, "_ssl");

            Assert.Equal("smtp.example.com", host);
            Assert.Equal("user@example.com", user);
            Assert.Equal("secret", password);
            Assert.Equal(587, port);
            Assert.True(ssl);
        }

        [Fact]
        public void ParsesPortAndSslFromConfig()
        {
            IConfiguration cfg = BuildConfig(new Dictionary<string, string>
            {
                {"Smtp:Host", "smtp.example.com"},
                {"Smtp:User", "user@example.com"},
                {"Smtp:Password", "secret"},
                {"Smtp:Port", "2525"},
                {"Smtp:Ssl", "false"}
            });

            GmailProvider provider = new(cfg);

            int port = (int)GetPrivateField(provider, "_port");
            bool ssl = (bool)GetPrivateField(provider, "_ssl");

            Assert.Equal(2525, port);
            Assert.False(ssl);
        }

        [Fact]
        public void InvalidPortFallsBackToDefault()
        {
            IConfiguration cfg = BuildConfig(new Dictionary<string, string>
            {
                {"Smtp:Host", "smtp.example.com"},
                {"Smtp:User", "user@example.com"},
                {"Smtp:Password", "secret"},
                {"Smtp:Port", "notanint"}
            });

            GmailProvider provider = new(cfg);
            int port = (int)GetPrivateField(provider, "_port");
            Assert.Equal(587, port);
        }

        [Fact]
        public void SslParsingIsCaseInsensitive()
        {
            IConfiguration cfgTrue = BuildConfig(new Dictionary<string, string>
            {
                {"Smtp:Host", "smtp.example.com"},
                {"Smtp:User", "user@example.com"},
                {"Smtp:Password", "secret"},
                {"Smtp:Ssl", "TRUE"}
            });

            IConfiguration cfgFalse = BuildConfig(new Dictionary<string, string>
            {
                {"Smtp:Host", "smtp.example.com"},
                {"Smtp:User", "user@example.com"},
                {"Smtp:Password", "secret"},
                {"Smtp:Ssl", "False"}
            });

            GmailProvider providerTrue = new(cfgTrue);
            GmailProvider providerFalse = new(cfgFalse);

            bool sslTrue = (bool)GetPrivateField(providerTrue, "_ssl");
            bool sslFalse = (bool)GetPrivateField(providerFalse, "_ssl");

            Assert.True(sslTrue);
            Assert.False(sslFalse);
        }

        [Fact]
        public async Task SendsEmailWithConfiguredProvider()
        {
            if (string.IsNullOrEmpty(_config["Smtp:Host"]) ||
                string.IsNullOrEmpty(_config["Smtp:User"]) ||
                string.IsNullOrEmpty(_config["Smtp:Password"]) ||
                string.IsNullOrEmpty(_config["Mail:From"]) ||
                string.IsNullOrEmpty(_config["Smtp:To"]))
            {
                return;
            }
            GmailProvider provider = new(_config);

            MailRequest mail = new(
                _config["Mail:From"]!,
                _config["Smtp:To"]!,
                "BeChallenge Integration Test",
                $"Integration test message at {DateTime.UtcNow:O}"
            );
            using CancellationTokenSource cts = new(TimeSpan.FromSeconds(60));
            await provider.SendEmail(mail, cts.Token);
        }
    }
}