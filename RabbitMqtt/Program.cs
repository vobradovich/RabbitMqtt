using MQTTnet.AspNetCore;
using RabbitMqtt.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHealthChecks();

builder.Services
    .AddMqttServer(mqttServer => mqttServer.WithoutDefaultEndpoint())
    .AddConnections();

builder.Services.AddRabbitConnectionFactory(c =>
{
    c.UserName = "";
    c.Password = "";
    c.AutomaticRecoveryEnabled = true;
    c.TopologyRecoveryEnabled = false;
    c.DispatchConsumersAsync = true;
    c.ClientProvidedName = nameof(RabbitMqtt);
});
builder.Services.AddRabbitProducerService();
builder.Services.AddSingleton<ConsumerServiceFactory>();
builder.Services.AddHostedService<MqttService>();

var app = builder.Build();

var mqttEndpoint = app.Configuration.GetValue("MqttEndpoint", "/mqtt/ws");
app.MapGet("/", () => $"Hello MQTT client! Connect to {mqttEndpoint}.");
app.MapMqtt(mqttEndpoint);

app.Run();
