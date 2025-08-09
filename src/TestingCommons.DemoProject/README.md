# TestingCommons Demo Project

This project demonstrates how to use TestingCommons packages with TestContainers for integration testing. It provides a complete setup for testing with MongoDB, PostgreSQL, and RabbitMQ using containerized databases.

## Overview

The DemoProject includes:
- **TestContainers Setup** - Automated container management for databases
- **Test Data** - Sample datasets for MongoDB and PostgreSQL
- **Integration Tests** - Real tests against containerized databases
- **TestingCommons Integration** - Examples using all TestingCommons packages

## Project Structure

```
TestingCommons.DemoProject/
├── Infrastructure/
│   └── TestContainersSetup.cs          # TestContainers configuration
├── Tests/
│   └── IntegrationTests.cs             # Integration tests with containers
├── test-data/                          # Sample test datasets
│   ├── mongo/                          # MongoDB test data
│   │   ├── employees.json              # Employee data (MongoDB format)
│   │   ├── products.json               # Product data (MongoDB format)
│   │   ├── orders.json                 # Order data (MongoDB format)
│   │   └── README.md                   # MongoDB data documentation
│   └── postgres/                       # PostgreSQL test data
│       ├── complete_dump.sql           # Complete DB dump
│       ├── schema.sql                  # Schema only
│       └── data.sql                    # Data only
└── TestingCommons.DemoProject.csproj   # Project configuration
```

## Features

### TestContainers Integration
- **MongoDB Container** - Automatic setup with test data import
- **PostgreSQL Container** - Schema creation and data seeding
- **RabbitMQ Container** - Message queues and exchanges setup
- **Automatic Cleanup** - Containers are disposed after tests

### Test Data Management
- **MongoDB JSON** - Ready-to-import JSON files with realistic data
- **PostgreSQL SQL** - Complete schema with normalized tables
- **Cross-Database Compatibility** - Same logical data in both formats
- **Realistic Relationships** - Orders reference customers and products

### TestingCommons Packages Demonstrated
- **TestingCommons.Core** - DateTime providers, utilities
- **TestingCommons.MongoDb** - MongoDB client abstractions
- **TestingCommons.RabbitMq** - Message queue operations
- **TestingCommons.RestApiClient** - HTTP client utilities

## Getting Started

### Prerequisites
- .NET 8.0 SDK
- Docker Desktop (for TestContainers)
- 4GB+ available RAM (for containers)

### Running the Tests

```bash
# Run all tests (will start containers automatically)
dotnet test

# Run specific test methods
dotnet test --filter "MongoDB_CanConnectAndQueryData"
dotnet test --filter "RabbitMQ_CanConnectAndPublishMessages"
dotnet test --filter "AllContainers_AreHealthyAndResponsive"

# Run with verbose output
dotnet test --logger:console;verbosity=detailed
```

### Test Execution Flow

1. **OneTimeSetUp** - Starts all containers and imports test data
2. **Test Execution** - Runs tests against live containers
3. **OneTimeTearDown** - Stops and removes containers

## TestContainers Configuration

The `TestContainersSetup` class provides:

```csharp
public class TestContainersSetup : IAsyncDisposable
{
    // Connection properties
    public string MongoConnectionString { get; }
    public string PostgresConnectionString { get; }
    public string RabbitMqHost { get; }
    public int RabbitMqPort { get; }
    public string RabbitMqUsername { get; }
    public string RabbitMqPassword { get; }

    // Container management
    public async Task StartContainersAsync();
    public async ValueTask DisposeAsync();
}
```

### MongoDB Container
- **Image**: `mongo:7.0`
- **Database**: `testingcommons_db`
- **Collections**: employees, products, orders
- **Authentication**: mongoadmin/mongopass
- **Features**: Automatic JSON import from test-data/mongo/

### PostgreSQL Container
- **Image**: `postgres:15`
- **Database**: `testingcommons_db`
- **Username**: testuser/testpass
- **Schema**: Normalized tables with foreign keys
- **Features**: SQL script execution from test-data/postgres/

### RabbitMQ Container
- **Image**: `rabbitmq:3.12-management`
- **Username**: rabbitmquser/rabbitmqpass
- **Management UI**: Available on random port
- **Features**: Exchanges, queues, bindings pre-configured

## Example Usage

### Basic Integration Test
```csharp
[Test]
public async Task MongoDB_CanConnectAndQueryData()
{
    var client = new MongoClient(_containers.MongoConnectionString);
    var database = client.GetDatabase("testingcommons_db");
    var employeesCollection = database.GetCollection<dynamic>("employees");

    var employeeCount = await employeesCollection.CountDocumentsAsync(FilterDefinition<dynamic>.Empty);
    var employees = await employeesCollection.Find(FilterDefinition<dynamic>.Empty).ToListAsync();

    Assert.That(employeeCount, Is.GreaterThan(0), "Should have employees in the database");
    Assert.That(employees, Is.Not.Empty, "Should retrieve employee documents");
}
```

### RabbitMQ Messaging Test
```csharp
[Test]
public async Task RabbitMQ_CanConnectAndPublishMessages()
{
    var factory = new ConnectionFactory()
    {
        HostName = _containers.RabbitMqHost,
        Port = _containers.RabbitMqPort,
        UserName = _containers.RabbitMqUsername,
        Password = _containers.RabbitMqPassword
    };

    await using var connection = await factory.CreateConnectionAsync();
    await using var channel = await connection.CreateChannelAsync();

    var testMessage = "Test message from integration test";
    var body = System.Text.Encoding.UTF8.GetBytes(testMessage);

    await channel.BasicPublishAsync(
        exchange: "testingcommons.events",
        routingKey: "employees.created",
        body: body);

    var queueName = "test.employees.created";
    var result = await channel.BasicGetAsync(queueName, autoAck: true);

    Assert.That(result, Is.Not.Null, "Should receive the published message");
    
    var receivedMessage = System.Text.Encoding.UTF8.GetString(result.Body.ToArray());
    Assert.That(receivedMessage, Is.EqualTo(testMessage), "Message content should match");
}
```

## Test Data Details

### Sample Data Volumes
- **5 Employees** - Different departments, skills, projects
- **4 Products** - Various categories with reviews
- **3 Orders** - Different statuses and payment methods
- **Relational Links** - Orders reference customers and products

### MongoDB Collections
```json
{
  "employees": "Employee profiles with skills and projects (5 records)",
  "products": "Product catalog with specifications and reviews (4 records)", 
  "orders": "Order history with items and payment details (3 records)"
}
```

### PostgreSQL Tables
```sql
-- Normalized schema
employees, employee_projects, products, product_reviews, 
orders, order_items

-- Features
- UUID primary keys
- Foreign key constraints  
- Check constraints
- Indexes for performance
- JSONB for flexible data
- Array types for lists
```

## Performance Considerations

### Container Startup Time
- **First Run**: 2-5 minutes (image download)
- **Subsequent Runs**: 30-60 seconds (cached images)
- **Parallel Tests**: Use `[Parallelizable(ParallelScope.None)]`

### Resource Usage
- **MongoDB**: ~512MB RAM
- **PostgreSQL**: ~256MB RAM  
- **RabbitMQ**: ~256MB RAM
- **Total**: ~1GB RAM + overhead

### Optimization Tips
```csharp
// Reuse containers across test class
[OneTimeSetUp]
public async Task Setup() => await _containers.StartContainersAsync();

[OneTimeTearDown] 
public async Task Cleanup() => await _containers.DisposeAsync();

// All tests use the same container instances
[Test]
public async Task MongoDB_CanConnectAndQueryData() { /* ... */ }

[Test]
public async Task RabbitMQ_CanConnectAndPublishMessages() { /* ... */ }

[Test]
public async Task AllContainers_AreHealthyAndResponsive() { /* ... */ }
```

## Troubleshooting

### Common Issues

**Docker not running**
```bash
# Windows
Start-Service docker

# macOS/Linux
systemctl start docker
```

**Port conflicts**
- TestContainers uses random ports automatically
- Check `docker ps` for running containers

**Memory issues**
- Increase Docker memory limit to 4GB+
- Close other Docker containers

**Slow startup**
- Run `docker pull mongo:latest postgres:15 rabbitmq:3-management` first
- Use SSD storage for better performance

### Debugging
```csharp
// Enable TestContainers logging
var logger = LoggerFactory.Create(builder => 
    builder.AddConsole().SetMinimumLevel(LogLevel.Debug))
    .CreateLogger<TestContainersSetup>();

// Check container status
await using var containers = new TestContainersSetup(logger);
await containers.StartContainersAsync();

// Connection strings are logged during startup
```

## CI/CD Integration

### GitHub Actions
```yaml
- name: Setup Docker
  run: |
    docker --version
    docker-compose --version

- name: Run Integration Tests  
  run: dotnet test
  env:
    DOCKER_HOST: tcp://localhost:2376
```

### Azure DevOps
```yaml
- task: DockerInstaller@0
  displayName: 'Install Docker'

- script: dotnet test
  displayName: 'Run Integration Tests'
```

This demo project provides a complete foundation for integration testing with TestingCommons and TestContainers!
