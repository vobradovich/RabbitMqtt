using MQTTnet;

namespace RabbitMqtt.Contracts;

public interface IConsumerService : IAsyncDisposable
{
    ValueTask StartAsync(Func<MqttApplicationMessage, CancellationToken, ValueTask> consumer, CancellationToken cancellationToken = default);
    ValueTask Subscribe(string mqttTopic);
    ValueTask Unsubscribe(string mqttTopic);
}
