namespace RabbitMqtt.Logging;

public static class RabbitConsumerServiceLogging
{
    private static readonly Action<ILogger, Exception?> _createConnectionError = LoggerMessage.Define(
        LogLevel.Error,
        new EventId(3041, nameof(CreateConnectionError)),
        "RabbitMQ Create Connection Error");
    public static void CreateConnectionError(this ILogger logger, Exception ex)
        => _createConnectionError(logger, ex);

    private static readonly Action<ILogger, string, string, string, string, Exception?> _consumerServiceStarted = LoggerMessage.Define<string, string, string, string>(
        LogLevel.Information,
        new EventId(3001, nameof(ConsumerServiceStarted)),
        "RabbitMQ Consumer Service Started ClientId = {ClientId}, TopicExchange = {TopicExchange}, QueueName = {QueueName}, ConsumerTag = {ConsumerTag}");
    public static void ConsumerServiceStarted(this ILogger logger, string clientId, string topicExchange, string queueName, string consumerTag)
        => _consumerServiceStarted(logger, clientId, topicExchange, queueName, consumerTag, null);

    private static readonly Action<ILogger, string, Exception?> _consumerServiceStopped = LoggerMessage.Define<string>(
        LogLevel.Information,
        new EventId(3002, nameof(ConsumerServiceStopped)),
        "RabbitMQ Consumer Service Stopped ClientId = {ClientId},");
    public static void ConsumerServiceStopped(this ILogger logger, string clientId)
        => _consumerServiceStopped(logger, clientId, null);

    private static readonly Action<ILogger, string, string, long, Exception?> _messageConsumed = LoggerMessage.Define<string, string, long>(
        LogLevel.Debug,
        new EventId(3020, nameof(MessageConsumed)),
        "RabbitMQ Message Consumed ClientId = {ClientId}, Topic = {Topic} in {ElapsedMilliseconds}ms");
    public static void MessageConsumed(this ILogger logger, string clientId, string topic, long elapsedMilliseconds)
        => _messageConsumed(logger, clientId, topic, elapsedMilliseconds, null);

    private static readonly Action<ILogger, string, long, Exception?> _messageConsumeError = LoggerMessage.Define<string, long>(
        LogLevel.Error,
        new EventId(3021, nameof(MessageConsumeError)),
        "RabbitMQ Message Consume Error ClientId = {ClientId} in {ElapsedMilliseconds}ms");
    public static void MessageConsumeError(this ILogger logger, string clientId, long elapsedMilliseconds, Exception ex)
        => _messageConsumeError(logger, clientId, elapsedMilliseconds, ex);

    private static readonly Action<ILogger, string, string, long, Exception?> _messageNack = LoggerMessage.Define<string, string, long>(
        LogLevel.Warning,
        new EventId(3021, nameof(MessageNack)),
        "RabbitMQ Message Nack ClientId = {ClientId}, Reason = {Reason} in {ElapsedMilliseconds}ms");
    public static void MessageNack(this ILogger logger, string clientId, string reason, long elapsedMilliseconds)
        => _messageNack(logger, clientId, reason, elapsedMilliseconds, null);

}
