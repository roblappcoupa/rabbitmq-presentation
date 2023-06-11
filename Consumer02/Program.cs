using Common;
using System.Text.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

var factory = new ConnectionFactory { HostName = "localhost" };

using var connection = factory.CreateConnection();
using var channel = connection.CreateModel();

channel.ExchangeDeclare(exchange: "topic_logs", type: ExchangeType.Topic);

channel.BasicQos(prefetchSize: 0, prefetchCount: 1, global: false);

// Declare a server-named queue
var queueName = channel.QueueDeclare().QueueName;

// Setup a binding for each desired topic
foreach (var bindingKey in args)
{
    channel.QueueBind(
        queue: queueName,
        exchange: "topic_logs",
        routingKey: bindingKey);
}

Console.WriteLine("Waiting for messages...");

var consumer = new EventingBasicConsumer(channel);
consumer.Received += (model, ea) =>
{
    var body = ea.Body.ToArray();

    var message = JsonSerializer.Deserialize<Message>(body);
    if (message == null)
    {
        Console.WriteLine("Received invalid or unexpected message format. Skipping processing");
        return;
    }

    Console.WriteLine("Received message. Id: {0}, Delay: {1}, Message: {2}", message.Id, message.DelayInSeconds, message.Text);

    Thread.Sleep(TimeSpan.FromSeconds(message.DelayInSeconds ?? 5));

    Console.WriteLine("Completed processing message {0}", message.Id);

    // Manual acknowledgement
    channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
};

channel.BasicConsume(
    queue: queueName,
    autoAck: false,
    consumer: consumer);

Console.WriteLine("Press any key to exit");
Console.ReadLine();