using System.Text;
using Common;
using System.Text.Json;
using System.Text.RegularExpressions;
using RabbitMQ.Client;

var factory = new ConnectionFactory { HostName = "localhost" };
using var connection = factory.CreateConnection();
using var channel = connection.CreateModel();

channel.ExchangeDeclare(exchange: "topic_logs", type: ExchangeType.Topic);

var id = 0;

while (true)
{
    Console.WriteLine("Enter input or press 'X' to exit:");

    var input = Console.ReadLine();
    if (input is null or "x")
    {
        break;
    }

    var regex = new Regex(@"--Message (?<Message>.*?) --Delay (?<Delay>\d+) --Topic (?<Topic>.*)");
    var match = regex.Match(input);
    if (!match.Success)
    {
        Console.WriteLine("Invalid input. The input must be in this format: --Message <string> --Delay <int> --Topic <string>");
        continue;
    }

    var text = match.Groups["Message"].Value;
    var delay = int.Parse(match.Groups["Delay"].Value);
    var topic = match.Groups["Topic"].Value;

    var message = new Message
    {
        Id = ++id,
        Text = text,
        DelayInSeconds = delay
    };

    var jsonString = JsonSerializer.Serialize(message);

    var body = Encoding.UTF8.GetBytes(jsonString);

    channel.BasicPublish(
        exchange: "topic_logs",
        routingKey: topic,
        basicProperties: null,
        body: body);


    Console.WriteLine("Sent message");
}
