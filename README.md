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
will pass the message `Hello, World` to the consumer which will simualte 10 seconds of "processing" time.

### Example 2: Multiple Queues
To run the producer, open a command prompt at the root and execute this command just like in the first example:
```
dotnet run --project ./Producer02/Producer02.csproj
```
To run the consumer, open a command prompt at the root and execute this command, passing one or more queue names as command line arguments:
```
dotnet run --project ./Consumer02/Consumer02.csproj  -- Queue01 Queue02 Queue03
```
After running one or both commands, navigate to `http://localhost:15672`, use the default credentials `guest`/`guest` and navigate to the queues tab.
You should have a queue for each command line argument that was passed. In the example above, you should have 3 queues: `Queue01`, `Queue02` and `Queue03`.

To produce messages, follow the same format as above with the addition of a routing key:
```
--Message <string> --Delay <int> -- RoutingKey <string>
```
`<string>` should be replaced with a string message, `<int>` should be replaced with a positive integer that represents the total number of seconds that the conumser should simulate "doing work" and the final `<string>` should be replaced with the names of one of the queues that was provided when starting the consumer. For example, entering this:
```
--Message Hello, World --Delay 10 --RoutingKey Queue02
```
will pass the message `Hello, World` to the `Queue02` consumer which will simualte 10 seconds of "processing" time.
