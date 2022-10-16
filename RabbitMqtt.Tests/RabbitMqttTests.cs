using System.Net.WebSockets;
using System.Text.Json;
using Microsoft.AspNetCore.TestHost;
using MQTTnet;
using MQTTnet.Adapter;
using MQTTnet.Client;
using MQTTnet.Diagnostics;
using MQTTnet.Formatter;
using MQTTnet.Implementations;

namespace RabbitMqtt.Tests;

public class RabbitMqttTests : IDisposable
{
    private readonly RabbitMqttApplication _application;
    private readonly HttpClient _client;
    private readonly WebSocketClient _socketClient;

    public RabbitMqttTests(ITestOutputHelper output)
    {
        _application = new RabbitMqttApplication(output);
        _client = _application.CreateClient();
        _socketClient = _application.Server.CreateWebSocketClient();
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        //_application?.Dispose();
    }

    [Fact]
    public async Task Get()
    {
        var response = await _client.GetAsync("/").ConfigureAwait(false);
        Assert.True(response.IsSuccessStatusCode);
    }

    [Fact]
    public async Task MqttWs()
    {
        var webSocket = await _socketClient.ConnectAsync(new Uri("ws://localhost/mqtt/ws"), CancellationToken.None).ConfigureAwait(false);
    }

    [Fact(Skip = "Not working")]
    public async Task MqttClient_Connect()
    {
        var uri = new Uri("ws://localhost/mqtt/ws");
        var webSocket = await _socketClient.ConnectAsync(uri, CancellationToken.None).ConfigureAwait(false);
        var clientAdapterFactory = new MqttWsClientAdapterFactory(webSocket, uri);
        var deviceId = "12345678abcd";
        var deviceType = "terminal";

        var mqttFactory = new MqttFactory();
        using var mqttClient = mqttFactory.CreateMqttClient(clientAdapterFactory);
        var mqttClientOptions = new MqttClientOptionsBuilder()
            .WithClientId(deviceId)
            .WithConnectionUri(new Uri("ws://localhost/mqtt/ws"))
            .WithWillTopic($"rabbitmq/device/disconnect/{deviceId}")
            .WithWillPayload(JsonSerializer.Serialize(new { deviceId, deviceType }))
            .Build();

        var response = await mqttClient.ConnectAsync(mqttClientOptions, CancellationToken.None);

        MqttClientSubscribeOptions subscribeOptions = new()
        {
            TopicFilters =
            {
                new MQTTnet.Packets.MqttTopicFilter { Topic = $"mqtt/device/{deviceId}" }
            }
        };
        var subscribe = await mqttClient.SubscribeAsync(subscribeOptions);

        var correlationId = Guid.NewGuid().ToString();

        var topic = $"rabbitmq/device/connect/{deviceId}/{correlationId}";
        await mqttClient.PublishStringAsync(topic, JsonSerializer.Serialize(new { deviceId, deviceType }));
    }
}

public sealed class MqttWsClientAdapterFactory : IMqttClientAdapterFactory
{
    private readonly WebSocket _webSocket;
    private readonly Uri _uri;

    public MqttWsClientAdapterFactory(WebSocket webSocket, Uri uri)
    {
        _webSocket = webSocket;
        _uri = uri;
    }

    public IMqttChannelAdapter CreateClientAdapter(MqttClientOptions options, MqttPacketInspector packetInspector, IMqttNetLogger logger)
    {
        if (options == null) throw new ArgumentNullException(nameof(options));

        var channel = new MqttWebSocketChannel(_webSocket, _uri.AbsoluteUri, _uri.AbsoluteUri.StartsWith("wss://", StringComparison.OrdinalIgnoreCase), null);
        var bufferWriter = new MqttBufferWriter(options.WriterBufferSize, options.WriterBufferSizeMax);
        var packetFormatterAdapter = new MqttPacketFormatterAdapter(options.ProtocolVersion, bufferWriter);
        return new MqttChannelAdapter(channel, packetFormatterAdapter, packetInspector, logger);
    }
}
