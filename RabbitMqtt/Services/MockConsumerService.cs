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
        return new ValueTask();
    }

    public ValueTask StartAsync(Func<MqttApplicationMessage, CancellationToken, ValueTask> consumer, CancellationToken cancellationToken = default)
    {
        return new ValueTask();
    }

    public ValueTask Subscribe(string mqttTopic)
    {
        return new ValueTask();
    }

    public ValueTask Unsubscribe(string mqttTopic)
    {
        return new ValueTask();
    }
}