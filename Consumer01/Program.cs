using System.Text.Json;
using Common;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

var factory = new ConnectionFactory { HostName = "localhost" };
using var connection = factory.CreateConnection();
using var channel = connection.CreateModel();

// Note: queue name matches what's being used in the message routing key
channel.QueueDeclare(
    queue: "consumer01",
    durable: false,
    exclusive: false,
    autoDelete: false,
    arguments: null);

channel.BasicQos(prefetchSize: 0, prefetchCount: 1, global: false);

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

    channel.BasicAck(ea.DeliveryTag, false);
};

// Note:
// 1. autoAck is set to true
// 2. queue name matches the message routing key set on the producer side
channel.BasicConsume(
    queue: "consumer01",
    autoAck: false,
    consumer: consumer);

Console.WriteLine("Press any key to exit");
Console.ReadLine();