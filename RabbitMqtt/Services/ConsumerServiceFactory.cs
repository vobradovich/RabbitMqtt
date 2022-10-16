using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMqtt.Contracts;
using RabbitMqtt.Options;

namespace RabbitMqtt.Services;

public class ConsumerServiceFactory
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<ConsumerServiceFactory> _logger;
    private IAutorecoveringConnection? _connection;

    public ConsumerServiceFactory(IServiceProvider serviceProvider, ILogger<ConsumerServiceFactory> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public IConsumerService CreateConsumerService(string connectionId)
    {
        return string.Equals(connectionId, MqttService.SenderClientId, StringComparison.InvariantCultureIgnoreCase)
            ? CreateMockService(connectionId)
            : CreateRabbitService(connectionId);
    }

    private IConsumerService CreateMockService(string connectionId)
    {
        var logger = _serviceProvider.GetRequiredService<ILoggerFactory>().CreateLogger<MockConsumerService>();
        return new MockConsumerService(connectionId, logger);
    }

    private IConsumerService CreateRabbitService(string connectionId)
    {
        var logger = _serviceProvider.GetRequiredService<ILoggerFactory>().CreateLogger<RabbitConsumerService>();
        var options = _serviceProvider.GetRequiredService<IOptions<RabbitMqOptions>>();
        _connection ??= CreateConnection();
        return new RabbitConsumerService(connectionId, _connection, options, logger);
    }

    private IAutorecoveringConnection CreateConnection()
    {
        try
        {
            var f = _serviceProvider.GetRequiredService<ConnectionFactory>();
            return (IAutorecoveringConnection)f.CreateConnection();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "RabbitMQ Connect Error");
            throw;
        }
    }
}
