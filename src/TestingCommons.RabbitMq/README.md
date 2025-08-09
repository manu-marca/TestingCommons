# TestingCommons.RabbitMq

The RabbitMq project provides a comprehensive client for RabbitMQ message broker operations. It supports both message publishing and consumption with flexible configuration options, making it ideal for testing message-driven architectures.

## Key Components

### **RabbitMqClient**
- Complete message broker client supporting publish and consume operations
- Multiple constructor overloads for different configuration scenarios
- Automatic connection health monitoring and startup
- Built-in consumer management and graceful shutdown

### **RabbitMqConfig**
- Comprehensive configuration for RabbitMQ connections
- Support for clusters, TLS connections, and authentication
- Configurable heartbeat intervals and redelivery settings

### **BusControl**
- Factory methods for creating MassTransit bus instances
- Support for advanced receive endpoint configurations
- Extensions for RabbitMQ-specific settings

## Usage Examples

### Basic Configuration
```csharp
var config = new RabbitMqConfig
{
    Hostname = "localhost",
    Username = "guest",
    Password = "guest",
    VirtualHost = "/",
    Port = 5672, // or 5671 for TLS
    HeartbeatInterval = 60
};

using var client = new RabbitMqClient(config);
```

### Publishing Messages
```csharp
// Simple message publishing
var message = new OrderCreated 
{ 
    OrderId = 123, 
    Amount = 99.99m, 
    CreatedAt = DateTime.UtcNow 
};

var requestId = client.PublishMessage(message);
Console.WriteLine($"Published message with ID: {requestId}");

// Publishing with redelivery count
var retryRequestId = client.PublishMessage(message, redeliveryCount: 2);
```

### Simple Message Handler
```csharp
var config = new RabbitMqConfig
{
    Hostname = "localhost",
    Username = "guest",
    Password = "guest",
    VirtualHost = "/"
};

using var client = new RabbitMqClient(config);

// Simple message consumption with lambda
var handle = client.ConsumeMessages<MyMessage>("my-queue", async message =>
{
    Console.WriteLine($"Received: {message.Content}");
    // Process the message
});

// Keep the consumer running
await client.WaitForMessages(TimeSpan.FromMinutes(5));

// Stop the specific consumer when done
await handle.StopAsync();
```

### Context-Aware Message Handler
```csharp
// Access to the full consume context (headers, message properties, etc.)
var handle = client.ConsumeMessages<MyMessage>("my-queue", async (message, context) =>
{
    Console.WriteLine($"Received: {message.Content}");
    Console.WriteLine($"Message ID: {context.MessageId}");
    Console.WriteLine($"Request ID: {context.RequestId}");
    
    // Access headers
    if (context.Headers.TryGetHeader("CustomHeader", out var headerValue))
    {
        Console.WriteLine($"Custom Header: {headerValue}");
    }
});
```

## Key Features

1. **Dual Operation Mode**: Support for both publishing and consuming messages
2. **Multiple Consumer Types**: Simple lambda handlers, context-aware handlers, and custom consumer classes
3. **Flexible Configuration**: Comprehensive RabbitMQ settings including TLS, clustering, and authentication
4. **Connection Management**: Automatic health monitoring, startup, and graceful shutdown
5. **MassTransit Integration**: Built on top of MassTransit for robust message handling
6. **Individual Consumer Control**: `HostReceiveEndpointHandle` return type allows granular consumer management
7. **Bulk Consumer Operations**: Stop all consumers at once or manage them individually
8. **Error Handling**: Access to message context for retry logic and error handling
9. **Testing-Optimized**: Designed specifically for testing message-driven architectures

## Method Return Types

- **`ConsumeMessages<T>()` methods**: Return `HostReceiveEndpointHandle` for individual consumer management
- **`PublishMessage<T>()` method**: Returns `Guid` request ID for message tracking
- **`WaitForMessages()` method**: Returns `Task` for awaiting message processing
- **`StopConsumers()` method**: Returns `Task` for graceful shutdown of all consumers
- **`HostReceiveEndpointHandle.StopAsync()` method**: Async method for graceful consumer shutdown
