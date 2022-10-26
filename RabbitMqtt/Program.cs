using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using MQTTnet.AspNetCore;
using RabbitMQ.Client;
using RabbitMqtt.Options;
using RabbitMqtt.Services;
using static RabbitMqtt.Options.EnvironmentVariables;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;
var services = builder.Services;
services
    .AddHealthChecks()
    .AddRabbitMQ(sp => sp.GetRequiredService<ConnectionFactory>().CreateAutorecoveringConnection("rabbitmqtt:healthcheck"), tags: new[] { "live" });

services
    .AddMqttServer(mqttServer => mqttServer.WithoutDefaultEndpoint())
    .AddConnections();

services.AddRabbitConnectionFactory(c =>
{
    c.UserName = configuration.GetValue<string>(RabbitMq.RABBITMQ_LOGIN);
    c.Password = configuration.GetValue<string>(RabbitMq.RABBITMQ_PASSWORD);
    c.HostName = configuration.GetValue(RabbitMq.RABBITMQ_HOST, "localhost");
    c.VirtualHost = configuration.GetValue(RabbitMq.RABBITMQ_VHOST, "/");
    c.AutomaticRecoveryEnabled = true;
    c.TopologyRecoveryEnabled = false;
    c.DispatchConsumersAsync = true;
    c.ClientProvidedName = nameof(RabbitMqtt);
});
services.AddRabbitProducerService();
services.AddSingleton<ConsumerServiceFactory>();
services.AddHostedService<MqttService>();

services.Configure<RabbitMqOptions>(c =>
{
    c.TopicExchange = configuration.GetValue(RabbitMq.RABBITMQ_EXCHANGE, "amq.topic");
    c.QueuePrefix = configuration.GetValue(RabbitMq.RABBITMQ_QUEUE_PREFIX, nameof(RabbitMqtt));
    c.QueueType = configuration.GetValue(RabbitMq.RABBITMQ_QUEUE_TYPE, "quorum");
    c.QueueTtl = configuration.GetValue(RabbitMq.RABBITMQ_QUEUE_TTL, 60 * 60 * 1000);
});

var app = builder.Build();

var mqttEndpoint = app.Configuration.GetValue(Mqtt.MQTT_ENDPOINT, "/mqtt/ws");
app.MapGet("/", () => $"Hello MQTT client! Connect to {mqttEndpoint}.");
app.MapMqtt(mqttEndpoint);
app.MapHealthChecks("/live", new HealthCheckOptions() { Predicate = _ => _.Tags.Contains("live") });

app.Run();
