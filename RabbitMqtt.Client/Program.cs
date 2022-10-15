using System.Text.Json;
using MQTTnet;
using MQTTnet.Client;

Console.WriteLine("Hello, MQTT server!");

var deviceId = "12345678abcd";
var deviceType = "terminal";

var mqttFactory = new MqttFactory();
using var mqttClient = mqttFactory.CreateMqttClient();
var mqttClientOptions = new MqttClientOptionsBuilder()
    .WithClientId(deviceId)
    .WithWebSocketServer("localhost:5138/mqtt/ws")
    .WithWillTopic($"rabbitmq/device/disconnect/{deviceId}")
    .WithWillPayload(JsonSerializer.Serialize(new { deviceId, deviceType }))
    .Build();

mqttClient.ApplicationMessageReceivedAsync += e =>
{
    Console.WriteLine($"Received application message ClientId = {e.ClientId}, Topic = {e.ApplicationMessage.Topic}");
    return Task.CompletedTask;
};

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

Console.WriteLine("The MQTT client is connected.");
Console.ReadKey();

await mqttClient.DisconnectAsync();