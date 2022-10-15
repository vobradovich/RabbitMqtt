using MQTTnet.Server;

namespace RabbitMqtt.Logging;

public static class MqttServiceLogging
{
    private static readonly Action<ILogger, Exception?> _serverStarted = LoggerMessage.Define(
        LogLevel.Information,
        new EventId(1000, nameof(ServerStarted)),
        "Server started");
    public static void ServerStarted(this ILogger logger)
        => _serverStarted(logger, null);

    private static readonly Action<ILogger, Exception?> _serverStopped = LoggerMessage.Define(
        LogLevel.Information,
        new EventId(1001, nameof(ServerStopped)),
        "Server stopped");
    public static void ServerStopped(this ILogger logger)
        => _serverStopped(logger, null);

    private static readonly Action<ILogger, string, string, Exception?> _clientConnected = LoggerMessage.Define<string, string>(
        LogLevel.Information,
        new EventId(1010, nameof(ClientConnected)),
        "Client connected ClientId = {ClientId}, Endpoint = {Endpoint}");
    public static void ClientConnected(this ILogger logger, ClientConnectedEventArgs arg)
        => _clientConnected(logger, arg.ClientId, arg.Endpoint, null);

    private static readonly Action<ILogger, string, string, MqttClientDisconnectType, Exception?> _clientDisconnected = LoggerMessage.Define<string, string, MqttClientDisconnectType>(
        LogLevel.Information,
        new EventId(1010, nameof(ClientDisconnected)),
        "Client disconnected ClientId = {ClientId}, Endpoint = {Endpoint}, DisconnectType = {DisconnectType}");
    public static void ClientDisconnected(this ILogger logger, ClientDisconnectedEventArgs arg)
        => _clientDisconnected(logger, arg.ClientId, arg.Endpoint, arg.DisconnectType, null);

    private static readonly Action<ILogger, string, string, Exception?> _applicationMessageNotConsumed = LoggerMessage.Define<string, string>(
        LogLevel.Information,
        new EventId(1020, nameof(ApplicationMessageNotConsumed)),
        "Send message to RabbitMq ClientId = {ClientId}, Topic = {Topic}");
    public static void ApplicationMessageNotConsumed(this ILogger logger, ApplicationMessageNotConsumedEventArgs arg)
        => _applicationMessageNotConsumed(logger, arg.SenderId, arg.ApplicationMessage.Topic, null);

    private static readonly Action<ILogger, string, string, Exception?> _clientSubscribedTopic = LoggerMessage.Define<string, string>(
        LogLevel.Information,
        new EventId(1030, nameof(ClientSubscribedTopic)),
        "Client subscribed topic ClientId = {ClientId}, Topic = {Topic}");
    public static void ClientSubscribedTopic(this ILogger logger, ClientSubscribedTopicEventArgs arg)
        => _clientSubscribedTopic(logger, arg.ClientId, arg.TopicFilter.Topic, null);

    private static readonly Action<ILogger, string, string, Exception?> _clientUnsubscribedTopic = LoggerMessage.Define<string, string>(
        LogLevel.Information,
        new EventId(1031, nameof(ClientUnsubscribedTopic)),
        "Client unsubscribed topic ClientId = {ClientId}, Topic = {Topic}");
    public static void ClientUnsubscribedTopic(this ILogger logger, ClientUnsubscribedTopicEventArgs arg)
        => _clientUnsubscribedTopic(logger, arg.ClientId, arg.TopicFilter, null);
}
