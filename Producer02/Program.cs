using System.Text;
using Common;
using System.Text.Json;
using System.Text.RegularExpressions;
using RabbitMQ.Client;

var factory = new ConnectionFactory { HostName = "localhost" };
using var connection = factory.CreateConnection();
using var channel = connection.CreateModel();

const string exchangeName = "Example02_Exchange";

//channel.ExchangeDeclare(exchange: "topic_logs", type: ExchangeType.Topic);
channel.ExchangeDeclare(exchange: exchangeName, type: ExchangeType.Direct);

var id = 0;

while (true)
{
    Console.WriteLine("Enter input or press 'X' to exit:");

    var input = Console.ReadLine();
    if (input is null or "x")
    {
        break;
    }

    var regex = new Regex(@"--Message (?<Message>.*?) --Delay (?<Delay>\d+) --RoutingKey (?<RoutingKey>.*)");
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
