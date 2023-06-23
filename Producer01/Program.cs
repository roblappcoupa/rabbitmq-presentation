using System.Text;
using Common;
using System.Text.Json;
using System.Text.RegularExpressions;
using RabbitMQ.Client;

// Point to locally running RabbitMQ
var factory = new ConnectionFactory { HostName = Common.Constants.RabbitHost };

// Create a connection and a channel within that connection
using var connection = factory.CreateConnection();
using var channel = connection.CreateModel();

const string exchangeName = Common.Constants.ExchangeName;

// Create the exchange
channel.ExchangeDeclare(exchange: exchangeName, type: ExchangeType.Direct);

// Generate ids so we can easily identify messages in the examples
var id = 0;

var regex = new Regex(@"--Message (?<Message>.*?) --Delay (?<Delay>\d+) --RoutingKey (?<RoutingKey>.*)");

// Run indefinitely
while (true)
{
    Console.WriteLine("Enter input or type 'X' to exit, then press Enter:");

    var input = Console.ReadLine();
    if (input is null or "x")
    {
        break;
    }
    
    var match = regex.Match(input);
    if (!match.Success)
    {
        Console.WriteLine("Invalid input. The input must be in this format: --Message <string> --Delay <int> --RoutingKey <string>");
        continue;
    }

    var text = match.Groups["Message"].Value;
    var delay = int.Parse(match.Groups["Delay"].Value);
    var routingKey = match.Groups["RoutingKey"].Value;

    var message = new Message
    {
        Id = ++id,
        Text = text,
        DelayInSeconds = delay
    };

    // Create a JSON structure from the Message class
    var jsonString = JsonSerializer.Serialize(message);

    var body = Encoding.UTF8.GetBytes(jsonString);

    Console.WriteLine("Publishing message {0}", id);

    // Publish a message to the broker
    channel.BasicPublish(
        exchange: exchangeName,
        routingKey: routingKey,
        basicProperties: null,
        body: body);

    Console.WriteLine("Published message {0}\n", id);
}