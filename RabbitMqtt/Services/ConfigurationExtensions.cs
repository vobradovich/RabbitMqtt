using System.Threading.Channels;
using MQTTnet;
using RabbitMQ.Client;

namespace RabbitMqtt.Services;

public static class ConfigurationExtensions
{
    public static IServiceCollection AddRabbitConnectionFactory(this IServiceCollection services, Action<ConnectionFactory>? configure = null)
    {
        var factory = new ConnectionFactory();
        configure?.Invoke(factory);
        services.AddSingleton(factory);
        return services;
    }

    public static IServiceCollection AddRabbitProducerService(this IServiceCollection services, Action<BoundedChannelOptions>? configure = null)
    {
        var channelOptions = new BoundedChannelOptions(10000)
        {
            FullMode = BoundedChannelFullMode.Wait,
            SingleReader = true,
        };
        configure?.Invoke(channelOptions);
        var channel = Channel.CreateBounded<MqttApplicationMessage>(channelOptions);
        services.AddSingleton(channel);
        services.AddSingleton(svc => svc.GetRequiredService<Channel<MqttApplicationMessage>>().Reader);
        services.AddSingleton(svc => svc.GetRequiredService<Channel<MqttApplicationMessage>>().Writer);
        services.AddHostedService<RabbitProducerService>();
        return services;
    }
}
