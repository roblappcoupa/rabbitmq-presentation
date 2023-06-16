using System.Text.Json;
using Common;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

// Create connection and channel
var factory = new ConnectionFactory { HostName = "localhost" };
using var connection = factory.CreateConnection();
using var channel = connection.CreateModel();

const string exchangeName = "Example01_Exchange";
const string queueName = "Example01_Queue";
const string routingKey = "Key0";

// Ensure the exchange is created
channel.ExchangeDeclare(exchange: exchangeName, type: ExchangeType.Direct);

// Ensure the queue is created
channel.QueueDeclare(
    queue: queueName,
    durable: false,
    exclusive: false,
    autoDelete: false,
    arguments: null);

// Set prefetch count
channel.BasicQos(prefetchSize: 0, prefetchCount: 1, global: false);

channel.QueueBind(
    queue: queueName,
    exchange: exchangeName,
    routingKey: routingKey);

// Declare an event handler that will fire when a message is delivered
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

    // Simulate some work
    Thread.Sleep(TimeSpan.FromSeconds(message.DelayInSeconds ?? 5));

    Console.WriteLine("Completed processing message {0}\n", message.Id);

    // Manually acknowledge message
    channel.BasicAck(ea.DeliveryTag, false);
};

channel.BasicConsume(
    queue: queueName,
    autoAck: false,
    consumer: consumer);

// Keep the program running and processing messages until the user presses a key
Console.WriteLine("Waiting for messages... Press any key to exit");
Console.ReadLine();