using MQTTnet;
using RabbitMQ.Client;

namespace RabbitMqtt.Logging;

public static class RabbitServiceLogging
{
    private static readonly Action<ILogger, string, string, Exception?> _producerServiceStarting = LoggerMessage.Define<string, string>(
        LogLevel.Information,
        new EventId(2000, nameof(ProducerServiceStarting)),
        "RabbitMQ Producer service starting HostName = {HostName}, VirtualHost = {VirtualHost}");
    public static void ProducerServiceStarting(this ILogger logger, ConnectionFactory connectionFactory)
        => _producerServiceStarting(logger, connectionFactory.HostName, connectionFactory.VirtualHost, null);

    private static readonly Action<ILogger, string, string, Exception?> _producerServiceStarted = LoggerMessage.Define<string, string>(
        LogLevel.Information,
        new EventId(2001, nameof(ProducerServiceStarted)),
        "RabbitMQ Producer service started HostName = {HostName}, ClientProvidedName = {ClientProvidedName}");
    public static void ProducerServiceStarted(this ILogger logger, IAutorecoveringConnection connection)
        => _producerServiceStarted(logger, connection.Endpoint.HostName, connection.ClientProvidedName, null);

    private static readonly Action<ILogger, string, string, Exception?> _producerServiceStopping = LoggerMessage.Define<string, string>(
        LogLevel.Information,
        new EventId(2010, nameof(ProducerServiceStopping)),
        "RabbitMQ Producer service stopping HostName = {HostName}, ClientProvidedName = {ClientProvidedName}");
    public static void ProducerServiceStopping(this ILogger logger, IAutorecoveringConnection connection)
        => _producerServiceStopping(logger, connection.Endpoint.HostName, connection.ClientProvidedName, null);

    private static readonly Action<ILogger, string, string, Exception?> _producerServiceStopped = LoggerMessage.Define<string, string>(
        LogLevel.Information,
        new EventId(2011, nameof(ProducerServiceStopped)),
        "RabbitMQ Producer service stopped HostName = {HostName}, VirtualHost = {VirtualHost}");
    public static void ProducerServiceStopped(this ILogger logger, ConnectionFactory connectionFactory)
        => _producerServiceStopped(logger, connectionFactory.HostName, connectionFactory.VirtualHost, null);

    private static readonly Action<ILogger, string, Exception?> _produceMessageFailure = LoggerMessage.Define<string>(
        LogLevel.Error,
        new EventId(2022, nameof(ProduceMessageFailure)),
        "RabbitMQ Produce message Failure Topic = {Topic}");
    public static void ProduceMessageFailure(this ILogger logger, Exception ex, MqttApplicationMessage message)
        => _produceMessageFailure(logger, message.Topic, ex);

    private static readonly Action<ILogger, int, Exception?> _produceMessagRetry = LoggerMessage.Define<int>(
        LogLevel.Warning,
        new EventId(2021, nameof(ProduceMessageRetry)),
        "RabbitMQ Produce message Retry = {Retry}");
    public static void ProduceMessageRetry(this ILogger logger, Exception ex, int retry)
        => _produceMessagRetry(logger, retry, ex);

    private static readonly Action<ILogger, string, Exception?> _produceMessageSuccess = LoggerMessage.Define<string>(
        LogLevel.Debug,
        new EventId(2020, nameof(ProduceMessageSuccess)),
        "RabbitMQ Produce message Success Topic = {Topic}");
    public static void ProduceMessageSuccess(this ILogger logger, MqttApplicationMessage message)
        => _produceMessageSuccess(logger, message.Topic, null);

}
