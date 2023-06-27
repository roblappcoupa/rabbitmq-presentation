using Common;
using System.Text.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;


if (!args.Any())
{
    throw new ArgumentException("You must pass one or more queue names to run this example. Example from .NET CLI: dotnet run --project Consumer01\\Consumer01.csproj -- Queue0:Key0");
}

// Point to locally running RabbitMQ
var factory = new ConnectionFactory { HostName = Common.Constants.RabbitHost };

// Create a connection and a channel within that connection
using var connection = factory.CreateConnection();
using var channel = connection.CreateModel();

const string exchangeName = Common.Constants.ExchangeName;

// Create the exchange
channel.ExchangeDeclare(exchange: exchangeName, type: ExchangeType.Direct);

// Set a prefetch count
channel.BasicQos(prefetchSize: 0, prefetchCount: 1, global: false);

// For each command line argument...
for (var i = 0; i < args.Length; i++)
{
    // Parse argument, splitting by : character to separate one or more queue names and binding keys
    var arg = args[i];
    var entries = arg.Split(":", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

    string queueName;
    string bindingKey;
    
    if (entries.Length == 1)
    {
        queueName = bindingKey = entries[0];
    }
    else if (entries.Length == 2)
    {
        queueName = entries[0];
        bindingKey = entries[1];
    }
    else
    {
        throw new ArgumentException("You must pass one or more queue/key names. Example: dotnet run --project Consumer01\\Consumer01.csproj -- Queue01:Key01");
    }

    // Create the queue
    Console.WriteLine("Creating queue {0}", queueName);
    channel.QueueDeclare(
        queue: queueName,
        durable: false,
        exclusive: false,
        autoDelete: false,
        arguments: null);

    // Bind the queue to the exchange with the provided binding key
    Console.WriteLine("Binding queue {0} to exchange {1} using binding key {2}", queueName, exchangeName, bindingKey);
    channel.QueueBind(
        queue: queueName,
        exchange: exchangeName,
        routingKey: bindingKey);

    // Define what we want to do when a message is received
    var consumer = new EventingBasicConsumer(channel);
    consumer.Received += (model, ea) =>
    {
        var body = ea.Body.ToArray();

        // Deserialize body as JSON
        var message = JsonSerializer.Deserialize<Message>(body);
        if (message == null)
        {
            Console.WriteLine("Received invalid or unexpected message format. Skipping processing");
            return;
        }

        Console.WriteLine("Received message. Id: {0}, Delay: {1}, Message: {2}", message.Id, message.DelayInSeconds, message.Text);

        // We add this here to demo how consumers can handle Exceptions
        if (string.Equals(message.Text, "throw", StringComparison.OrdinalIgnoreCase))
        {
            Console.WriteLine(Common.Constants.ErrorMessage);
            throw new Exception(Common.Constants.ErrorMessage);
        }

        // Dumb example, just simulating processing time
        Thread.Sleep(TimeSpan.FromSeconds(message.DelayInSeconds ?? 5));

        Console.WriteLine("Completed processing message {0}", message.Id);

        // Manually acknowledge the message (signaling success)
        channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
    };

    Console.WriteLine("Basic consume on queue {0}", queueName);

    // Start consuming messages
    channel.BasicConsume(
        queue: queueName,
        autoAck: false,
        consumer: consumer);
}

Console.WriteLine("Waiting for messages... Press enter to exit");
Console.ReadLine();