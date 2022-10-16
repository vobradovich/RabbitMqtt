using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace RabbitMqtt.Tests;

internal class RabbitMqttApplication : WebApplicationFactory<Program>
{
    private readonly ITestOutputHelper _output;

    public RabbitMqttApplication(ITestOutputHelper output)
    {
        _output = output;
    }

    protected override IHost CreateHost(IHostBuilder builder)
    {
        builder.ConfigureHostConfiguration(AppConfiguration);
        builder.ConfigureServices(services =>
        {
        });
        builder.ConfigureLogging(logging =>
        {
            // remove other logging providers, such as remote loggers or unnecessary event logs
            logging.ClearProviders();
            logging.AddXunit(_output, new Divergic.Logging.Xunit.LoggingConfig { ScopeFormatter = new EmptyScopeFormatter()});
        });
        return base.CreateHost(builder);
    }

    protected virtual void AppConfiguration(IConfigurationBuilder builder)
    {
        builder
            .AddJsonFile("appsettings.Tests.json", optional: true)
            .AddInMemoryCollection(new Dictionary<string, string>
            {

            });
    }
}
