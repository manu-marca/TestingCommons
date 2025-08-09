using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using Npgsql;
using RabbitMQ.Client;
using TestingCommons.DemoProject.Infrastructure;
using TestingCommons.MongoDb;

namespace TestingCommons.DemoProject.Tests;

[TestFixture]
public class IntegrationTests
{
    private TestContainersSetup _containers = null!;
    private ILogger<TestContainersSetup> _logger = null!;

    [OneTimeSetUp]
    public async Task OneTimeSetUp()
    {
        var loggerFactory = LoggerFactory.Create(builder =>
            builder.AddConsole().SetMinimumLevel(LogLevel.Information));
        _logger = loggerFactory.CreateLogger<TestContainersSetup>();

        _containers = new TestContainersSetup(_logger);
        await _containers.StartContainersAsync();
    }

    [OneTimeTearDown]
    public async Task OneTimeTearDown()
    {
        if (_containers != null)
            await _containers.DisposeAsync();
    }

    [Test]
    public async Task MongoDB_CanConnectAndQueryData()
    {
        var client = new MongoClient(_containers.MongoConnectionString);
        var database = client.GetDatabase("testingcommons_db");
        var employeesCollection = database.GetCollection<dynamic>("employees");

        var collections = await (await database.ListCollectionNamesAsync()).ToListAsync();

        var employeeCount = await employeesCollection.CountDocumentsAsync(FilterDefinition<dynamic>.Empty);
        var employees = await employeesCollection.Find(FilterDefinition<dynamic>.Empty).ToListAsync();

        Assert.That(employeeCount, Is.GreaterThan(0), "Should have employees in the database");
        Assert.That(employees, Is.Not.Empty, "Should retrieve employee documents");
    }

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

    [Test]
    public async Task TestingCommons_MongoDbClient_WorksWithTestContainer()
    {
        var connectionString = _containers.MongoConnectionString;
        var databaseName = "testingcommons_db";
        
        var client = new MongoClient(connectionString);
        var database = client.GetDatabase(databaseName);
        
        var productsCollection = database.GetCollection<dynamic>("products");
        var products = await productsCollection
            .Find(FilterDefinition<dynamic>.Empty)
            .ToListAsync();

        Assert.That(products, Is.Not.Empty, "Should have products in the database");
    }

    [Test]
    public async Task AllContainers_AreHealthyAndResponsive()
    {
        var mongoClient = new MongoClient(_containers.MongoConnectionString);
        var mongoAdmin = mongoClient.GetDatabase("admin");
        var mongoPing = await mongoAdmin.RunCommandAsync<MongoDB.Bson.BsonDocument>("{ ping: 1 }");
        Assert.That(mongoPing["ok"].ToDouble(), Is.EqualTo(1.0), "MongoDB should be healthy");

        await using var pgConnection = new NpgsqlConnection(_containers.PostgresConnectionString);
        await pgConnection.OpenAsync();
        await using var pgCommand = new NpgsqlCommand("SELECT 1", pgConnection);
        var pgResult = await pgCommand.ExecuteScalarAsync();
        Assert.That(pgResult, Is.EqualTo(1), "PostgreSQL should be healthy");

        var rabbitFactory = new ConnectionFactory()
        {
            HostName = _containers.RabbitMqHost,
            Port = _containers.RabbitMqPort,
            UserName = _containers.RabbitMqUsername,
            Password = _containers.RabbitMqPassword
        };
        
        await using var rabbitConnection = await rabbitFactory.CreateConnectionAsync();
        Assert.That(rabbitConnection.IsOpen, Is.True, "RabbitMQ connection should be open");
    }

}
