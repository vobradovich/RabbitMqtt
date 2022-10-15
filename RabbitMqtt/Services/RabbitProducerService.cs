using System.Net;
using System.Threading.Channels;
using MQTTnet;
using Polly;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMqtt.Logging;

namespace RabbitMqtt.Services
{
    public class RabbitProducerService : BackgroundService
    {
        private readonly ChannelReader<MqttApplicationMessage> _channelReader;
        private readonly ChannelWriter<MqttApplicationMessage> _channelWriter;
        private readonly ConnectionFactory _connectionFactory;
        private readonly ILogger<RabbitProducerService> _logger;
        private IAutorecoveringConnection? _connection;
        private IModel? _channel;

        public RabbitProducerService(
            ChannelReader<MqttApplicationMessage> channelReader,
            ChannelWriter<MqttApplicationMessage> channelWriter,
            ConnectionFactory connectionFactory,
            ILogger<RabbitProducerService> logger)
        {
            _channelReader = channelReader;
            _channelWriter = channelWriter;
            _connectionFactory = connectionFactory;
            _logger = logger;
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            if (_connection != null)
            {
                _logger.ProducerServiceStopping(_connection);
            }
            if (_channelWriter.TryComplete())
            {
                await _channelReader.Completion;
            }
            if (_channel != null)
            {
                _channel.ModelShutdown -= ChannelShutdown;
                _channel.Close();
                _channel.Dispose();
                _channel = null;
            }
            if (_connection != null)
            {
                _connection.RecoverySucceeded -= RecoverySucceeded;
                _connection.ConnectionRecoveryError -= ConnectionRecoveryError;
                _connection.Close();
                _connection.Dispose();
                _connection = null;
            }
            _logger.ProducerServiceStopped(_connectionFactory);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.ProducerServiceStarting(_connectionFactory);
            _connection = (IAutorecoveringConnection)_connectionFactory.CreateConnection();
            _connection.RecoverySucceeded += RecoverySucceeded;
            _connection.ConnectionRecoveryError += ConnectionRecoveryError;
            _channel = _connection.CreateModel();
            _channel.ModelShutdown += ChannelShutdown;
            _logger.ProducerServiceStarted(_connection);

            var policy = Policy
                .Handle<Exception>()
                .WaitAndRetryForever(sleepDurationProvider: i => TimeSpan.FromSeconds(i < 5 ? i : 5), onRetry: OnRetry);

            await foreach (var message in _channelReader.ReadAllAsync(stoppingToken))
            {
                var result = policy.ExecuteAndCapture(() => Publish(message));
                if (result.Outcome == OutcomeType.Failure)
                {
                    _logger.ProduceMessageFailure(result.FinalException, message);
                }
            }
        }

        private void Publish(MqttApplicationMessage message)
        {
            var routingKey = message.Topic.Replace('/', '.');
            var replyTo = message.ResponseTopic?.Replace('/', '.');
            var properties = _channel!.CreateBasicProperties();

            //properties.Persistent = message.Persistent;
            //properties.Type = message.Type;
            if (!string.IsNullOrEmpty(replyTo))
            {
                properties.ReplyTo = replyTo;
            }
            //if (!string.IsNullOrEmpty(message.CorrelationId))
            //{
            //    properties.CorrelationId = message.CorrelationId;
            //}
            if (message.MessageExpiryInterval > 0)
            {
                properties.Expiration = (message.MessageExpiryInterval * 1000).ToString();
            }
            if (!string.IsNullOrEmpty(message.ContentType))
            {
                properties.ContentType = message.ContentType;
            }
            _channel.BasicPublish(
                exchange: "amq.topic",
                routingKey: routingKey,
                basicProperties: properties,
                body: message.Payload);
            _logger.ProduceMessageSuccess(message);
        }

        private void OnRetry(Exception ex, int retry, TimeSpan timeSpan)
        {
            if (_channel != null)
            {
                try
                {
                    _channel.ModelShutdown -= ChannelShutdown;
                    _channel.Close();
                    _channel.Dispose();
                }
                catch { }
            }
            _channel = _connection!.CreateModel();
            _channel.ModelShutdown += ChannelShutdown;
            _logger.ProduceMessageRetry(ex, retry);
        }

        private void ChannelShutdown(object? sender, ShutdownEventArgs e)
        {
            _logger.LogWarning("RabbitMQ ChannelShutdown {Info}", new { HostName = Dns.GetHostName(), e.Cause });
        }

        private void RecoverySucceeded(object? sender, EventArgs e)
        {
            _logger.LogWarning("RabbitMQ RecoverySucceeded {Info}", new { HostName = Dns.GetHostName() });
        }

        private void ConnectionRecoveryError(object? sender, ConnectionRecoveryErrorEventArgs e)
        {
            _logger.LogError(e.Exception, "RabbitMQ RecoveryError {Info}", new { HostName = Dns.GetHostName() });
        }
    }
}