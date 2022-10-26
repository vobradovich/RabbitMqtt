# RabbitMqtt
RabbitMqtt Proof of concept bridge RabbitMQ - MQTTnet

### General

* Replacement for [RabbitMQ Web MQTT Plugin](https://www.rabbitmq.com/web-mqtt.html)
* Reduce RabbitMQ Connection churn rate
* Use quorum queues with TTL

### MQTTnet
https://github.com/dotnet/MQTTnet

### Environment Variables 

|Variable|Default value|Description|
|---|---|---|
|RABBITMQ_LOGIN|||
|RABBITMQ_PASSWORD|||
|RABBITMQ_HOST|localhost||
|RABBITMQ_VHOST|/||
|RABBITMQ_EXCHANGE|amq.topic||
|RABBITMQ_QUEUE_PREFIX|RabbitMqtt||
|RABBITMQ_QUEUE_TYPE|quorum||
|RABBITMQ_QUEUE_TTL|60 * 60 * 1000||