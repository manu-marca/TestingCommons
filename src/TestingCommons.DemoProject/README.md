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
│   ├── employees.json                  # Employee data (MongoDB format)
│   ├── products.json                   # Product data (MongoDB format)
│   ├── orders.json                     # Order data (MongoDB format)
│   ├── postgres/                       # PostgreSQL test data
│   │   ├── complete_dump.sql           # Complete DB dump
│   │   ├── schema.sql                  # Schema only
│   │   └── data.sql                    # Data only
│   └── README.md                       # Test data documentation
├── BasicTests.cs                       # Basic unit tests
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

# Run specific test categories
dotnet test --filter "Category=Integration"
dotnet test --filter "Category=Unit"

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
- **Image**: `mongo:latest`
- **Database**: `testingcommons_db`
- **Collections**: employees, products, orders
- **Features**: Automatic JSON import, replica set support

### PostgreSQL Container
- **Image**: `postgres:15`
- **Database**: `testingcommons_db`
- **Schema**: Normalized tables with foreign keys
- **Features**: Custom types, indexes, triggers

### RabbitMQ Container
- **Image**: `rabbitmq:3-management`
- **Management UI**: Available on random port
- **Features**: Exchanges, queues, bindings pre-configured

## Example Usage

### Basic Integration Test
```csharp
[Test]
public async Task MongoDB_CanStoreAndRetrieveData()
{
    // Containers are already started in OneTimeSetUp
    var client = new MongoClient(_containers.MongoConnectionString);
    var database = client.GetDatabase("testingcommons_db");
    var collection = database.GetCollection<Employee>("employees");

    // Your test logic here
    var employees = await collection.Find(_ => true).ToListAsync();
    Assert.That(employees, Is.Not.Empty);
}
```

### Using TestingCommons.MongoDb
```csharp
[Test]
public async Task TestingCommons_MongoClient_Integration()
{
    var client = new MongoDbClientBase(
        _containers.MongoConnectionString, 
        "testingcommons_db");
    
    // Use TestingCommons abstractions
    var employees = await client.GetCollectionAsync<Employee>("employees");
    var activeEmployees = await employees
        .Find(e => e.IsActive)
        .ToListAsync();
    
    Assert.That(activeEmployees, Is.Not.Empty);
}
```

### PostgreSQL with Entity Framework
```csharp
[Test]
public async Task PostgreSQL_WithEntityFramework()
{
    var options = new DbContextOptionsBuilder<TestDbContext>()
        .UseNpgsql(_containers.PostgresConnectionString)
        .Options;
    
    await using var context = new TestDbContext(options);
    
    var departments = await context.Employees
        .GroupBy(e => e.Department)
        .Select(g => new { Department = g.Key, Count = g.Count() })
        .ToListAsync();
    
    Assert.That(departments, Is.Not.Empty);
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
  "employees": "Employee profiles with skills and projects",
  "products": "Product catalog with specifications and reviews", 
  "orders": "Order history with items and payment details"
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

// Use fast test data
[Test, Category("Fast")]
public void UnitTest_WithoutContainers() { /* ... */ }

[Test, Category("Integration")]
public async Task IntegrationTest_WithContainers() { /* ... */ }
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
  run: dotnet test --filter "Category=Integration"
  env:
    DOCKER_HOST: tcp://localhost:2376
```

### Azure DevOps
```yaml
- task: DockerInstaller@0
  displayName: 'Install Docker'

- script: dotnet test --filter "Category=Integration"
  displayName: 'Run Integration Tests'
```

This demo project provides a complete foundation for integration testing with TestingCommons and TestContainers!
