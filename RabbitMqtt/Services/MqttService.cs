using System.Collections.Concurrent;
using System.Threading.Channels;
using MQTTnet;
using MQTTnet.Server;
using RabbitMqtt.Contracts;
using RabbitMqtt.Logging;

namespace RabbitMqtt.Services;

public class MqttService : BackgroundService
{
    public static string SenderClientId { get; } = Guid.Empty.ToString();

    private readonly MqttServer _server;
    private readonly ConsumerServiceFactory _consumerServiceFactory;
    private readonly ChannelWriter<MqttApplicationMessage> _channelWriter;
    private readonly ILogger<MqttService> _logger;
    private readonly ConcurrentDictionary<string, IConsumerService> _clientConsumers = new();

    public MqttService(
        MqttServer server,
        ConsumerServiceFactory consumerServiceFactory,
        ChannelWriter<MqttApplicationMessage> channelWriter,
        ILogger<MqttService> logger)
    {
        _server = server;
        _consumerServiceFactory = consumerServiceFactory;
        _channelWriter = channelWriter;
        _logger = logger;

        server.StartedAsync += Server_StartedAsync;
        server.StoppedAsync += Server_StoppedAsync;

        server.ValidatingConnectionAsync += Server_ValidatingConnectionAsync;
        server.ClientConnectedAsync += Server_ClientConnectedAsync;
        server.ClientDisconnectedAsync += Server_ClientDisconnectedAsync;

        server.ClientSubscribedTopicAsync += Server_ClientSubscribedTopicAsync;
        server.ClientUnsubscribedTopicAsync += Server_ClientUnsubscribedTopicAsync;
        server.ClientAcknowledgedPublishPacketAsync += Server_ClientAcknowledgedPublishPacketAsync;

        server.ApplicationMessageNotConsumedAsync += Server_ApplicationMessageNotConsumedAsync;
        server.RetainedMessageChangedAsync += Server_RetainedMessageChangedAsync;
    }

    private Task Server_StartedAsync(EventArgs arg)
    {
        _logger.ServerStarted();
        return Task.CompletedTask;
    }

    private Task Server_StoppedAsync(EventArgs arg)
    {
        _logger.ServerStopped();
        return Task.CompletedTask;
    }

    private Task Server_ValidatingConnectionAsync(ValidatingConnectionEventArgs arg)
    {
        _logger.LogInformation(nameof(Server_ValidatingConnectionAsync));
        return Task.CompletedTask;
    }

    private async Task Server_ClientConnectedAsync(ClientConnectedEventArgs arg)
    {
        _logger.ClientConnected(arg);
        var cosumerService = _consumerServiceFactory.CreateConsumerService(arg.ClientId);
        if (_clientConsumers.TryAdd(arg.ClientId, cosumerService))
        {
            await cosumerService.StartAsync(Consumer);
        }
    }

    private async Task Server_ClientDisconnectedAsync(ClientDisconnectedEventArgs arg)
    {
        _logger.ClientDisconnected(arg);
        if (_clientConsumers.TryRemove(arg.ClientId, out var _consumerService))
        {
            await _consumerService.DisposeAsync();
        }
    }

    private Task Server_RetainedMessageChangedAsync(RetainedMessageChangedEventArgs arg)
    {
        _logger.LogInformation(nameof(Server_RetainedMessageChangedAsync));
        return Task.CompletedTask;
    }

    private async Task Server_ApplicationMessageNotConsumedAsync(ApplicationMessageNotConsumedEventArgs arg)
    {
        _logger.ApplicationMessageNotConsumed(arg);
        if (string.Equals(arg.SenderId, SenderClientId, StringComparison.InvariantCultureIgnoreCase))
        {
            return;
        }
        await _channelWriter.WriteAsync(arg.ApplicationMessage);
    }

    private Task Server_ClientAcknowledgedPublishPacketAsync(ClientAcknowledgedPublishPacketEventArgs arg)
    {
        _logger.LogInformation(nameof(Server_ClientAcknowledgedPublishPacketAsync));
        return Task.CompletedTask;
    }

    private async Task Server_ClientSubscribedTopicAsync(ClientSubscribedTopicEventArgs arg)
    {
        _logger.ClientSubscribedTopic(arg);
        if (_clientConsumers.TryGetValue(arg.ClientId, out var _consumerService))
        {
            await _consumerService.Subscribe(arg.TopicFilter.Topic);
        }
    }

    private async Task Server_ClientUnsubscribedTopicAsync(ClientUnsubscribedTopicEventArgs arg)
    {
        _logger.ClientUnsubscribedTopic(arg);
        if (_clientConsumers.TryGetValue(arg.ClientId, out var _consumerService))
        {
            await _consumerService.Unsubscribe(arg.TopicFilter);
        }
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        return Task.CompletedTask;
    }

    private async ValueTask Consumer(MqttApplicationMessage message, CancellationToken cancellationToken)
    {
        await _server.InjectApplicationMessage(new InjectedMqttApplicationMessage(message)
        {
            SenderClientId = SenderClientId,
        });
    }
}