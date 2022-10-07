using Address2Map;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Address2MapTests
{
    namespace NUnitTestCiscoService
    {
        public class MockWebApp : WebApplicationFactory<Startup>
        {
            private readonly string configFile = "appsettings.json";
            public MockWebApp(string configFile = "appsettings.json") : base()
            {
                if (!string.IsNullOrEmpty(configFile))
                {
                    this.configFile = configFile;
                }
            }
            protected override void ConfigureWebHost(IWebHostBuilder builder)
            {
                var configuration = new ConfigurationBuilder()
                    .AddJsonFile(configFile)
                    .Build();
                builder.ConfigureAppConfiguration(c =>
                {
                    c.AddConfiguration(configuration);
                });
                builder.ConfigureLogging((WebHostBuilderContext context, ILoggingBuilder loggingBuilder) =>
                {
                    loggingBuilder.ClearProviders();
                    loggingBuilder.AddConsole(options => options.IncludeScopes = true);
                });

                builder.UseConfiguration(configuration);
            }
        }
    }

}
