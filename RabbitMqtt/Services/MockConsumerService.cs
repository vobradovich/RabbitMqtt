using MQTTnet;
using RabbitMqtt.Contracts;

namespace RabbitMqtt.Services;

public class MockConsumerService : IConsumerService
{
    private readonly string _connectionId;
    private readonly ILogger<MockConsumerService> _logger;

    public MockConsumerService(string connectionId, ILogger<MockConsumerService> logger)
    {
        _connectionId = connectionId;
        _logger = logger;
    }

    public ValueTask DisposeAsync()
    {
        return ValueTask.CompletedTask;
    }

    public ValueTask StartAsync(Func<MqttApplicationMessage, CancellationToken, ValueTask> consumer, CancellationToken cancellationToken = default)
    {
        return ValueTask.CompletedTask;
    }

    public ValueTask Subscribe(string mqttTopic)
    {
        return ValueTask.CompletedTask;
    }

    public ValueTask Unsubscribe(string mqttTopic)
    {
        return ValueTask.CompletedTask;
    }
}