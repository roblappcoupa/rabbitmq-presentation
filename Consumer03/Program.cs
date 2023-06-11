using Common;
using EasyNetQ;

using var bus = RabbitHutch.CreateBus(
    "host=localhost;prefetchcount=1",
    x => x
        .EnableConsoleLogger()
        .EnableAlwaysNackWithoutRequeueConsumerErrorStrategy());

await bus.PubSub.SubscribeAsync<Message>(
    "console-app-0",
    async message =>
    {
        Console.WriteLine("Processing message {0}", message.Id);
        await Task.Delay(TimeSpan.FromSeconds(5));
        Console.WriteLine("Text:{0}", message.Text);
        Console.WriteLine("Finished processing message {0}", message.Id);
    });


Console.WriteLine("Press any key to exit");
Console.ReadLine();
