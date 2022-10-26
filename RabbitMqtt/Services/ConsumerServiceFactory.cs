using Microsoft.AspNetCore.Connections;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMqtt.Contracts;
using RabbitMqtt.Logging;
using RabbitMqtt.Options;

namespace RabbitMqtt.Services;

public class ConsumerServiceFactory
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ConnectionFactory _connectionFactory;
    private readonly ILogger<ConsumerServiceFactory> _logger;
    private IAutorecoveringConnection? _connection;

    public ConsumerServiceFactory(
        IServiceProvider serviceProvider,
        ConnectionFactory connectionFactory,
        ILogger<ConsumerServiceFactory> logger)
    {
        _serviceProvider = serviceProvider;
        _connectionFactory = connectionFactory;
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
            return _connectionFactory.CreateAutorecoveringConnection("rabbitmqtt:consumer");
        }
        catch (Exception ex)
        {
            _logger.CreateConnectionError(ex);
            throw;
        }
    }
}

public static class RabbitExtensions
{
    public static IAutorecoveringConnection CreateAutorecoveringConnection(this ConnectionFactory connectionFactory, string clientName)
    {
        var hostNames = connectionFactory.HostName.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        return (IAutorecoveringConnection)connectionFactory.CreateConnection(hostNames, clientName);
    }
}
