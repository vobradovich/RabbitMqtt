using System.Diagnostics;
using Microsoft.Extensions.Options;
using MQTTnet;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMqtt.Contracts;
using RabbitMqtt.Logging;
using RabbitMqtt.Options;

namespace RabbitMqtt.Services;

public class RabbitConsumerService : IConsumerService
{
    private readonly string _connectionId;
    private readonly IAutorecoveringConnection _connection;
    private readonly IOptions<RabbitMqOptions> _options;
    private readonly ILogger<RabbitConsumerService> _logger;

    private Func<MqttApplicationMessage, CancellationToken, ValueTask>? _consumer;
    private IModel? _channel;
    private string? _queueName;
    private CancellationToken _cancellationToken;

    public RabbitConsumerService(
        string connectionId,
        IAutorecoveringConnection connection,
        IOptions<RabbitMqOptions> options,
        ILogger<RabbitConsumerService> logger)
    {
        _connectionId = connectionId;
        _connection = connection;
        _options = options;
        _logger = logger;
    }

    public ValueTask StartAsync(Func<MqttApplicationMessage, CancellationToken, ValueTask> consumer, CancellationToken cancellationToken = default)
    {
        _consumer = consumer;
        _cancellationToken = cancellationToken;
        _connection.CallbackException += CallbackException;
        _connection.RecoverySucceeded += ConnectionRecovered;
        _connection.ConnectionRecoveryError += ConnectionRecoveryError;

        CloseChannel();
        CreateChannel();
        return ValueTask.CompletedTask;
    }

    private void CallbackException(object? sender, CallbackExceptionEventArgs args)
        => _logger.CallbackException(args);

    private void ConnectionRecovered(object? sender, EventArgs args)
    {
        _logger.ConnectionRecoverySucceeded();
        CloseChannel();
        CreateChannel();
    }

    private void ConnectionRecoveryError(object? sender, ConnectionRecoveryErrorEventArgs args)
        => _logger.ConnectionRecoveryError(args);

    private void CreateChannel()
    {
        _channel = _connection.CreateModel();
        _channel.BasicQos(0, 1, false);
        _queueName = _channel.QueueDeclare(
            $"{_options.Value.QueuePrefix}.{_connectionId}",
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: new Dictionary<string, object>
            {
                ["x-queue-type"] = _options.Value.QueueType,
                ["x-expires"] = _options.Value.QueueTtl,
            }).QueueName;

        var consumer = new AsyncEventingBasicConsumer(_channel);
        consumer.Received += Received;
        var consumerTag = _channel.BasicConsume(queue: _queueName, autoAck: false, exclusive: false, consumer: consumer);
        _logger.ConsumerServiceStarted(_connectionId, _options.Value.TopicExchange, _queueName, consumerTag);
    }


    private async Task Received(object sender, BasicDeliverEventArgs args)
    {
        var sw = Stopwatch.StartNew();
        var props = args.BasicProperties;
        using var logScope = _logger.BeginScope(new Dictionary<string, object>
        {
            ["correlationId"] = args.BasicProperties.CorrelationId,
            ["connectionId"] = _connectionId,
            ["rabbitmq"] = new { args.Exchange, args.RoutingKey, args.ConsumerTag, QueueName = _queueName },
        });
        if (!(_channel?.IsOpen ?? false) || _cancellationToken.IsCancellationRequested)
        {
            _channel?.BasicNack(args.DeliveryTag, false, true);
            _logger.MessageNack(_connectionId, "IsCancellationRequested", sw.ElapsedMilliseconds);
            return;
        }
        if (string.IsNullOrEmpty(args.RoutingKey))
        {
            _channel?.BasicNack(args.DeliveryTag, false, false);
            _logger.MessageNack(_connectionId, "RoutingKey IsNullOrEmpty", sw.ElapsedMilliseconds);
            return;
        }
        try
        {
            var topic = args.RoutingKey.Replace('.', '/');
            var responseTopic = props.ReplyTo?.Replace('.', '/');
            var messageBuilder = new MqttApplicationMessageBuilder()
                .WithPayload(args.Body.ToArray())
                .WithContentType(props.ContentType)
                .WithTopic(topic)
                .WithResponseTopic(responseTopic);
            var message = messageBuilder.Build();
            await _consumer!.Invoke(message, _cancellationToken);
            _logger.MessageConsumed(_connectionId, topic, sw.ElapsedMilliseconds);
        }
        catch (Exception ex)
        {
            _logger.MessageConsumeError(_connectionId, sw.ElapsedMilliseconds, ex);
        }
        finally
        {
            _channel?.BasicAck(args.DeliveryTag, false);
        }
    }

    public ValueTask DisposeAsync()
    {
        // Suppress finalization.
        GC.SuppressFinalize(this);
        _connection.CallbackException -= CallbackException;
        _connection.RecoverySucceeded -= ConnectionRecovered;
        _connection.ConnectionRecoveryError -= ConnectionRecoveryError;
        CloseChannel();
        return ValueTask.CompletedTask;
    }

    private void CloseChannel()
    {
        if (_channel?.IsOpen == true)
        {
            _logger.ConsumerServiceStopped(_connectionId);
            _channel.Close(StatusCodes.Status200OK, "Channel closed");
        }
        _channel?.Dispose();
        _channel = null;
    }

    public ValueTask Subscribe(string mqttTopic)
    {
        var routingKey = mqttTopic.Replace('/', '.');
        _channel?.QueueBind(_queueName, _options.Value.TopicExchange, routingKey);
        return ValueTask.CompletedTask;
    }

    public ValueTask Unsubscribe(string mqttTopic)
    {
        var routingKey = mqttTopic.Replace('/', '.');
        _channel?.QueueUnbind(_queueName, _options.Value.TopicExchange, routingKey);
        return ValueTask.CompletedTask;
    }
}
