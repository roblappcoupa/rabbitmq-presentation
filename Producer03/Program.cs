using Common;
using EasyNetQ;
using System.Text.RegularExpressions;

using var bus = RabbitHutch.CreateBus(
    "host=localhost",
    x => x
        .EnableConsoleLogger()
        .EnableAlwaysNackWithoutRequeueConsumerErrorStrategy());

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

    Console.WriteLine("Publishing message");
    await bus.PubSub.PublishAsync(message);
    Console.WriteLine("Message published");
}