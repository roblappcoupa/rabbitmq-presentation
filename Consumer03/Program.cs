using Common;
using EasyNetQ;

// Running two instances of the consumer with the same queue name results in one queue with two consumers:
// messages get round robin'd

// Running two instances of the consumer with the different queue name results in two queues with one consumer per queue:
// every message gets delivered to each queue
//
// Adding two handlers for the same type by calling SubscribeAsync<T> twice results in one queue, two consumers:
// messages are round robin'd
//
// Adding two handlers for different types by calling SubscribeAsync<T> twice results in two queues, two consumers:
// messages get routed to the appropriate queue

var subscriberId = args.Length > 0 ? args[0] : "DefaultSubscriberId";

using var bus = RabbitHutch.CreateBus(
    $"host=localhost;prefetchcount=1;product=Consumer_{subscriberId}",
    x => x
        .EnableConsoleLogger()
        .EnableNewtonsoftJson()
        .EnableAlwaysNackWithoutRequeueConsumerErrorStrategy());

await bus.PubSub.SubscribeAsync<Message>(
    subscriberId,
    async message =>
    {
        var delayInSeconds = message.DelayInSeconds ?? 5;

        Console.WriteLine("\n[0] Processing message {0}. Simulating {1} seconds of work", message.Id, delayInSeconds);
        await Task.Delay(TimeSpan.FromSeconds(delayInSeconds));
        Console.WriteLine("Finished processing message {0}\n", message.Id);
    });

Console.WriteLine("Waiting for messages... Press any key to exit");
Console.ReadLine();
