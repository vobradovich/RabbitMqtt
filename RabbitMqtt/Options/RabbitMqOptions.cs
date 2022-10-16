namespace RabbitMqtt.Options;

public record RabbitMqOptions
{
    /// <summary>
    /// RabbitMQ Topic exchange. Default: amq.topic
    /// </summary>
    /// <example>amq.topic</example>
    public string TopicExchange { get; set; } = "amq.topic";
    /// <summary>
    /// RabbitMQ queue prefix for MQTT connection. Queue name: {QueuePrefix}.{ConnectionId}
    /// </summary>
    /// <example>RabbitMqtt</example>
    public string QueuePrefix { get; set; } = nameof(RabbitMqtt);
    /// <summary>
    /// RabbitMQ queue type. Default: quorum
    /// <see cref="https://www.rabbitmq.com/quorum-queues.html"/>
    /// </summary>
    public string QueueType { get; set; } = "quorum";
    /// <summary>
    /// RabbitMQ queue TTL in milliseconds. Default: 60 min.
    /// <see cref="https://www.rabbitmq.com/ttl.html#queue-ttl"/>
    /// </summary>
    public int QueueTtl { get; set; } = 60 * 60 * 1000; // 60 minutes
}
