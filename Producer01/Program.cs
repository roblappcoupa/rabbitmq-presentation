using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using Common;
using RabbitMQ.Client;

var factory = new ConnectionFactory { HostName = "localhost" };
using var connection = factory.CreateConnection();
using var channel = connection.CreateModel();

// We always ensure the queue is created
// This ensures that if the producer starts before the consumer, the messages will be available
// in the queue for the producer once it starts
channel.QueueDeclare(
    queue: "consumer01",
    durable: false,
    exclusive: false,
    autoDelete: false,
    arguments: null);

//var properties = channel.CreateBasicProperties();
//properties.Persistent = true;

var id = 0;

while (true)
{
    Console.WriteLine("Enter input or press 'X' to exit:");

    var input = Console.ReadLine();
    if (input is null or "x" or "X")
    {
        break;
    }

    var regex = new Regex(@"--Message (?<Message>.*?) --Delay (?<Delay>\d+)");
    var match = regex.Match(input);
    if (!match.Success)
    {
        Console.WriteLine("Invalid input. The input must be in this format: --Message <string> --Delay <int>");
        continue;
    }

    var text = match.Groups["Message"].Value;
    var delay = int.Parse(match.Groups["Delay"].Value);

    var message = new Message
    {
        Id = ++id,
        Text = text,
        DelayInSeconds = delay
    };

    var jsonString = JsonSerializer.Serialize(message);

    var body = Encoding.UTF8.GetBytes(jsonString);

    Console.WriteLine("Publishing message {0}", id);

    channel.BasicPublish(
        exchange: string.Empty,
        routingKey: "consumer01",
        basicProperties: null,
        body: body);

    Console.WriteLine("Published message {0}\n", id);
}
