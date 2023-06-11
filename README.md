# RabbitMQ

### Setup
Run RabbitMQ in a Docker container

```
docker run -d --name local-rabbit -p 5672:5672 -p 5673:5673 -p 15672:15672 rabbitmq:3-management
```

