using System.Collections.Generic;
using System.IO;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.Identity.Client;

[assembly: FunctionsStartup(typeof(IssuerApi.Startup))]

namespace IssuerApi
{
    public class Startup : FunctionsStartup
    {
        public override void ConfigureAppConfiguration(IFunctionsConfigurationBuilder builder)
        {
            var context = builder.GetContext();

            builder.ConfigurationBuilder
                .AddJsonFile(Path.Combine(context.ApplicationRootPath, "appsettings.json"), optional: true, reloadOnChange: false)
                .AddJsonFile(Path.Combine(context.ApplicationRootPath, $"appsettings.{context.EnvironmentName}.json"), optional: true, reloadOnChange: false)
                //.AddAzureAppConfiguration(Environment.GetEnvironmentVariable("AZMAN-AAC-CONNECTION"), optional: true)
                //.AddUserSecrets(Assembly.GetExecutingAssembly(), true)
                .AddEnvironmentVariables();
        }

        public override void Configure(IFunctionsHostBuilder builder)
        {
            builder.Services.AddHttpClient();

            builder.Services.AddOptions<MsalTokenProviderConfiguration>()
                .Configure<IConfiguration>((options, configuration) =>
                {
                    configuration.GetSection("AzureAd").Bind(options);
                });

            builder.Services.AddSingleton<MsalTokenProvider>();
            builder.Services.AddOptions<VerifiableCredentialServiceConfiguration>()
                .Configure<IConfiguration>((options, configuration) =>
                {
                    configuration.GetSection("VC").Bind(options);
                    options.ValidCredentialTypes = options.ValidCredentialTypeSetting.Split(',');
                });
        }
    }

    public class MsalTokenProviderConfiguration
    {
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public string TenantId { get; set; }
        public string Authority { get; set; }
        public string Scopes { get; set; }
    }

    public class VerifiableCredentialServiceConfiguration
    {
        public string Endpoint { get; set; }
        public string ServiceScope { get; set; }
        public string Instance { get; set; }
        public string IssuerAuthority { get; set; }
        public string VerifierAuthority { get; set; }
        public string CredentialManifest { get; set; }
        public string CallbackUrl { get => $"{HostName}/{CallbackPath}"; }
        public string CallbackPath { get; set; }
        public string HostName { get; set; }
        public string CredentialTypeBase { get; set; }
        public string ClientName { get; set; }
        public string ValidCredentialTypeSetting { get; set; }
        public string[] ValidCredentialTypes { get; set; }
    }

    public class MsalTokenProvider
    {
        private readonly MsalTokenProviderConfiguration _config;
        public readonly IConfidentialClientApplication _client;

        public MsalTokenProvider(IOptions<MsalTokenProviderConfiguration> opts)
        {
            _config = opts.Value;

            _client = ConfidentialClientApplicationBuilder
                    .Create(_config.ClientId)
                    .WithClientSecret(_config.ClientSecret)
                    .WithAuthority(_config.Authority ?? $"https://login.microsoftonline.com/{_config.TenantId}/v2.0")
                    .Build();
        }
    }
}
