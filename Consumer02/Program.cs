using Common;
using System.Text.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;


if (!args.Any())
{
    throw new ArgumentException("You must pass one or more queue names to run this example. Example from .NET CLI: dotnet run --project Consumer02\\Consumer02.csproj -- Queue01 Queue02 Queue03");
}

var factory = new ConnectionFactory { HostName = "localhost" };

using var connection = factory.CreateConnection();
using var channel = connection.CreateModel();

const string exchangeName = "Example02_Exchange";

channel.ExchangeDeclare(exchange: exchangeName, type: ExchangeType.Direct);

channel.BasicQos(prefetchSize: 0, prefetchCount: 1, global: false);
// channel.BasicQos(prefetchSize: 0, prefetchCount: 2, global: false); // false: per-consumer, true: per-channel

for (var i = 0; i < args.Length; i++)
{
    var queueName = args[i];

    Console.WriteLine("Creating queue {0}", queueName);
    channel.QueueDeclare(
        queue: queueName,
        durable: false,
        exclusive: false,
        autoDelete: false,
        arguments: null);

    Console.WriteLine("Binding queue {0} to exchange {1} using binding key {2} (same as the queue name)", queueName, exchangeName, queueName);
    channel.QueueBind(
        queue: queueName,
        exchange: exchangeName,
        routingKey: queueName); // In this example, we are giving the queue the same routing key as the queue name

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

    Console.WriteLine("Basic consume on queue {0}", queueName);

    // Start consuming messages
    channel.BasicConsume(
        queue: queueName,
        autoAck: false,
        consumer: consumer);
}

Console.WriteLine("Waiting for messages... Press any key to exit");
Console.ReadLine();