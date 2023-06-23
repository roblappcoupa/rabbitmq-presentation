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
dotnet run --project ./Consumer01/Consumer01.csproj -- Queue0:Key0
```
After running one or both commands, navigate to `http://localhost:15672`, use the default credentials `guest`/`guest` and navigate to the queues tab.
You should have a single queue named `Queue0` that is bound to an exchange called `Example01_Exchange` with routing key `Key0`.

To produce messages, open the command prompt window that is running the producer and enter a message in this format
```
--Message <string> --Delay <int> -- RoutingKey <string>
```
`<string>` should be replaced with a string message, `<int>` should be replaced with a positive integer that represents the total number of seconds that the conumser should simulate "doing work" and the final `<string>` should be replaced with the names of one of the queues that was provided when starting the consumer. For example, entering this:
```
--Message Hello, World --Delay 10 --RoutingKey Key0
```
You will see the message being handled by the producer.
