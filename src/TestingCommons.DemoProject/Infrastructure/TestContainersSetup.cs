using MongoDB.Driver;
using Npgsql;
using Testcontainers.MongoDb;
using Testcontainers.PostgreSql;
using Testcontainers.RabbitMq;
using RabbitMQ.Client;
using Microsoft.Extensions.Logging;

namespace TestingCommons.DemoProject.Infrastructure;

public class TestContainersSetup : IAsyncDisposable
{
    private readonly ILogger<TestContainersSetup> _logger;
    
    private MongoDbContainer? _mongoContainer;
    private PostgreSqlContainer? _postgresContainer;
    private RabbitMqContainer? _rabbitMqContainer;
    
    public string MongoConnectionString { get; private set; } = string.Empty;
    public string PostgresConnectionString { get; private set; } = string.Empty;
    public string RabbitMqConnectionString { get; private set; } = string.Empty;
    public string RabbitMqHost { get; private set; } = string.Empty;
    public int RabbitMqPort { get; private set; }
    public string RabbitMqUsername { get; private set; } = string.Empty;
    public string RabbitMqPassword { get; private set; } = string.Empty;

    public TestContainersSetup(ILogger<TestContainersSetup> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Start all containers and initialize databases
    /// </summary>
    public async Task StartContainersAsync()
    {
        _logger.LogInformation("Starting TestContainers...");

        var tasks = new[]
        {
            StartMongoDbAsync(),
            StartPostgreSqlAsync(),
            StartRabbitMqAsync()
        };

        await Task.WhenAll(tasks);

        _logger.LogInformation("All TestContainers started successfully!");
        LogConnectionDetails();
    }

    private async Task StartMongoDbAsync()
    {
        _logger.LogInformation("Starting MongoDB container...");
        
        _mongoContainer = new MongoDbBuilder()
            .WithImage("mongo:7.0")
            .WithUsername("mongoadmin")
            .WithPassword("mongopass")
            .WithPortBinding(27017, true)
            .Build();

        await _mongoContainer.StartAsync();
        MongoConnectionString = _mongoContainer.GetConnectionString();
        
        _logger.LogInformation($"MongoDB started at: {MongoConnectionString}");
        
        await InitializeMongoDbAsync();
    }

    private async Task StartPostgreSqlAsync()
    {
        _logger.LogInformation("Starting PostgreSQL container...");
        
        _postgresContainer = new PostgreSqlBuilder()
            .WithImage("postgres:15")
            .WithDatabase("testingcommons_db")
            .WithUsername("testuser")
            .WithPassword("testpass")
            .WithPortBinding(5432, true)
            .Build();

        await _postgresContainer.StartAsync();
        PostgresConnectionString = _postgresContainer.GetConnectionString();
        
        _logger.LogInformation($"PostgreSQL started at: {PostgresConnectionString}");
        
        await InitializePostgreSqlAsync();
    }

    private async Task StartRabbitMqAsync()
    {
        _logger.LogInformation("Starting RabbitMQ container...");
        
        _rabbitMqContainer = new RabbitMqBuilder()
            .WithImage("rabbitmq:3.12-management")
            .WithUsername("rabbitmquser")
            .WithPassword("rabbitmqpass")
            .WithPortBinding(5672, true)
            .WithPortBinding(15672, true)
            .Build();

        await _rabbitMqContainer.StartAsync();
        
        RabbitMqHost = _rabbitMqContainer.Hostname;
        RabbitMqPort = _rabbitMqContainer.GetMappedPublicPort(5672);
        RabbitMqUsername = "rabbitmquser";
        RabbitMqPassword = "rabbitmqpass";
        RabbitMqConnectionString = $"amqp://{RabbitMqUsername}:{RabbitMqPassword}@{RabbitMqHost}:{RabbitMqPort}/";
        
        _logger.LogInformation($"RabbitMQ started at: {RabbitMqConnectionString}");
        _logger.LogInformation($"RabbitMQ Management UI: http://{RabbitMqHost}:{_rabbitMqContainer.GetMappedPublicPort(15672)}");
        
        await InitializeRabbitMqAsync();
    }

    private async Task InitializeMongoDbAsync()
    {
        try
        {
            _logger.LogInformation("Initializing MongoDB with test data...");
            
            var client = new MongoClient(MongoConnectionString);
            var database = client.GetDatabase("testingcommons_db");
            
            var testDataPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "test-data");
            _logger.LogInformation($"Looking for test data at: {testDataPath}");
            
            if (!Directory.Exists(testDataPath))
            {
                _logger.LogWarning($"Test data directory not found at: {testDataPath}");
                return;
            }
            
            await ImportMongoCollectionAsync(database, "employees", Path.Combine(testDataPath, "mongo", "employees.json"));
            await ImportMongoCollectionAsync(database, "products", Path.Combine(testDataPath, "mongo", "products.json"));
            await ImportMongoCollectionAsync(database, "orders", Path.Combine(testDataPath, "mongo", "orders.json"));
            
            _logger.LogInformation("MongoDB test data initialized successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initialize MongoDB test data");
        }
    }

    private async Task ImportMongoCollectionAsync(IMongoDatabase database, string collectionName, string jsonFilePath)
    {
        if (!File.Exists(jsonFilePath))
        {
            _logger.LogWarning($"Test data file not found: {jsonFilePath}");
            return;
        }

        _logger.LogInformation($"Importing data from: {jsonFilePath}");
        var json = await File.ReadAllTextAsync(jsonFilePath);
        
        if (string.IsNullOrWhiteSpace(json))
        {
            _logger.LogWarning($"Test data file is empty: {jsonFilePath}");
            return;
        }
        
        var documents = MongoDB.Bson.Serialization.BsonSerializer.Deserialize<MongoDB.Bson.BsonDocument[]>(json);
        
        var collection = database.GetCollection<MongoDB.Bson.BsonDocument>(collectionName);
        await collection.InsertManyAsync(documents);
        
        _logger.LogInformation($"Imported {documents.Length} documents into {collectionName} collection");
    }

    private async Task InitializePostgreSqlAsync()
    {
        try
        {
            _logger.LogInformation("Initializing PostgreSQL with test data...");
            
            var testDataPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "test-data", "postgres");
            var sqlFilePath = Path.Combine(testDataPath, "complete_dump.sql");
            
            if (!File.Exists(sqlFilePath))
            {
                _logger.LogWarning($"PostgreSQL test data file not found: {sqlFilePath}");
                return;
            }

            await using var connection = new NpgsqlConnection(PostgresConnectionString);
            await connection.OpenAsync();
            
            var sql = await File.ReadAllTextAsync(sqlFilePath);
            
            var statements = sql.Split(new[] { ";\r\n", ";\n" }, StringSplitOptions.RemoveEmptyEntries);
            
            foreach (var statement in statements)
            {
                var trimmedStatement = statement.Trim();
                if (string.IsNullOrEmpty(trimmedStatement) || trimmedStatement.StartsWith("--"))
                    continue;

                await using var command = new NpgsqlCommand(trimmedStatement, connection);
                await command.ExecuteNonQueryAsync();
            }
            
            _logger.LogInformation("PostgreSQL test data initialized successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initialize PostgreSQL test data");
        }
    }

    private async Task InitializeRabbitMqAsync()
    {
        try
        {
            _logger.LogInformation("Initializing RabbitMQ with test queues...");
            
            var factory = new ConnectionFactory()
            {
                HostName = RabbitMqHost,
                Port = RabbitMqPort,
                UserName = RabbitMqUsername,
                Password = RabbitMqPassword
            };

            await using var connection = await factory.CreateConnectionAsync();
            await using var channel = await connection.CreateChannelAsync();
            
            var testQueues = new[]
            {
                "test.employees.created",
                "test.employees.updated",
                "test.orders.placed",
                "test.orders.shipped",
                "test.products.inventory.low",
                "test.notifications.email"
            };

            foreach (var queueName in testQueues)
            {
                await channel.QueueDeclareAsync(
                    queue: queueName,
                    durable: true,
                    exclusive: false,
                    autoDelete: false,
                    arguments: null);
                
                _logger.LogInformation($"Created queue: {queueName}");
            }
            
            await channel.ExchangeDeclareAsync(
                exchange: "testingcommons.events",
                type: ExchangeType.Topic,
                durable: true);
            
            foreach (var queueName in testQueues)
            {
                var routingKey = queueName.Replace("test.", "");
                await channel.QueueBindAsync(queueName, "testingcommons.events", routingKey);
            }
            
            _logger.LogInformation("RabbitMQ test queues and exchanges initialized successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initialize RabbitMQ test setup");
        }
    }

    private void LogConnectionDetails()
    {
        _logger.LogInformation("\n=== TestContainers Connection Details ===");
        _logger.LogInformation($"MongoDB: {MongoConnectionString}");
        _logger.LogInformation($"PostgreSQL: {PostgresConnectionString}");
        _logger.LogInformation($"RabbitMQ: {RabbitMqConnectionString}");
        _logger.LogInformation($"RabbitMQ Management: http://{RabbitMqHost}:{_rabbitMqContainer?.GetMappedPublicPort(15672)}");
        _logger.LogInformation("==========================================\n");
    }

    public async ValueTask DisposeAsync()
    {
        _logger.LogInformation("Stopping TestContainers...");

        var disposeTasks = new List<Task>();

        if (_mongoContainer != null)
            disposeTasks.Add(_mongoContainer.DisposeAsync().AsTask());
        
        if (_postgresContainer != null)
            disposeTasks.Add(_postgresContainer.DisposeAsync().AsTask());
        
        if (_rabbitMqContainer != null)
            disposeTasks.Add(_rabbitMqContainer.DisposeAsync().AsTask());

        await Task.WhenAll(disposeTasks);
        
        _logger.LogInformation("All TestContainers stopped and disposed");
    }
}
