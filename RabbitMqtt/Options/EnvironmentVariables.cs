namespace RabbitMqtt.Options;

public static class EnvironmentVariables
{
    public static class RabbitMq
    {
        public const string RABBITMQ_HOST = nameof(RABBITMQ_HOST);
        public const string RABBITMQ_VHOST = nameof(RABBITMQ_VHOST);
        public const string RABBITMQ_LOGIN = nameof(RABBITMQ_LOGIN);
        public const string RABBITMQ_PASSWORD = nameof(RABBITMQ_PASSWORD);
        public const string RABBITMQ_EXCHANGE = nameof(RABBITMQ_EXCHANGE);
        public const string RABBITMQ_QUEUE_PREFIX = nameof(RABBITMQ_QUEUE_PREFIX);
        public const string RABBITMQ_QUEUE_TYPE = nameof(RABBITMQ_QUEUE_TYPE);
        public const string RABBITMQ_QUEUE_TTL = nameof(RABBITMQ_QUEUE_TTL);
    }

    public static class Mqtt
    {
        public const string MQTT_ENDPOINT = nameof(MQTT_ENDPOINT);
    }
}
