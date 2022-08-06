using Azure.Identity;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.DependencyInjection;
using System;

[assembly: FunctionsStartup(typeof(AzDOEpicPauseChildren.Startup))]


namespace AzDOEpicPauseChildren
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            builder.Services.AddAzureClients(clientBuilder =>
            {
                clientBuilder.AddSecretClient(new Uri(Environment.GetEnvironmentVariable("KeyVaultUri")));
                clientBuilder.UseCredential(new DefaultAzureCredential());
            });
        }
    }
}
