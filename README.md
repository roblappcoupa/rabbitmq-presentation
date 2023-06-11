# RabbitMQ

### Setup
Run RabbitMQ in a Docker container

```
docker run -d --name local-rabbit -p 5672:5672 -p 5673:5673 -p 15672:15672 rabbitmq:3-management
```

### Example 1: Basic Publishing and Consuming
To run the producer, open a command prompt at the root and execute this command:
```
dotnet run --project ./Producer01/Producer01.csproj
```
To run a consumer, open a command prompt at the root and execute this command:
```
dotnet run --project ./Consumer01/Consumer01.csproj
```
After running one or both commands, navigate to `http://localhost:15672`, use the default credentials `guest`/`guest` and navigate to the queues tab.
You should have a single queue named `consumer01`.

In the producer application, you can start producing messages. By default, the `Consumer01` application has a pre-fetch count of 1.
Since auto-acknowledgement is turned on, messages will be immediately delivered to the consumer from the broker, but they will be processed one at a time by the application's event handler.
To produce a message, follow this format:
```
--Message <string> --Delay <int>
```
`<string>` should be replaced with a string message and `<int>` should be replaced with a positive integer that represents the total number of seconds that the conumser should simulate "doing work". For example, entering this:
```
--Message Hello, World --Delay 10
```
will pass the message `Hello, World` to the consumer and cause the consumer to simualte 10 seconds of "processing" time.
Experiments: pre-fetch count, auto-acknowledgements
