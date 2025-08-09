# TestingCommons.MongoDb

The MongoDb project provides a robust base client for MongoDB operations in testing scenarios. It includes pre-configured serialization, transaction support, and comprehensive configuration options.

## Key Components

### **MongoDbClientBase**
- Abstract base class for MongoDB client implementations
- Pre-configured with standard BSON serialization conventions
- Supports TLS 1.2 connections and SSL settings
- Automatic configuration of camel case conventions and enum string representation

### **MongoDbConfiguration**
- Comprehensive configuration class for MongoDB settings
- Connection string, database name, logging options
- Transaction support configuration
- Read preference modes and staleness settings
- Message consume delay configuration

### **Transaction Support**
- `MongoDbTransactionOptions` - Configuration for MongoDB transactions
- Read/Write concern level management
- Support for distributed transactions when enabled

### **Serialization Provider**
- Custom BSON serialization for .NET types
- Decimal serialization as BSON Decimal128
- DateTimeOffset serialization as string
- Proper handling of nullable types

## Usage Examples

### Basic Configuration
```csharp
var mongoConfig = new MongoDbConfiguration
{
    ConnectionString = "mongodb://localhost:27017",
    Database = "TestDatabase",
    EnableCommandTextLogging = true,
    EnableMongoDbConnectionDiagnostics = true,
    HasTransactionSupportEnabled = true
};
```

### Creating a Custom Client
```csharp
public class MyMongoClient : MongoDbClientBase
{
    public MyMongoClient(MongoDbConfiguration config) : base(config)
    {
    }

    public IMongoCollection<TDocument> GetCollection<TDocument>(string name)
    {
        return GetDatabase().GetCollection<TDocument>(name);
    }

    public async Task<List<TDocument>> FindAllAsync<TDocument>(string collectionName)
    {
        var collection = GetCollection<TDocument>(collectionName);
        return await collection.Find(_ => true).ToListAsync();
    }
}

// Usage
var client = new MyMongoClient(mongoConfig);
var users = await client.FindAllAsync<User>("users");
```

### Transaction Configuration
```csharp
var transactionOptions = new MongoDbTransactionOptions
{
    ReadConcern = ReadConcernLevel.Majority,
    WriteConcern = "majority" // Can be "majority", "1", "2", etc.
};

var config = new MongoDbConfiguration
{
    ConnectionString = "mongodb://localhost:27017",
    Database = "TestDatabase",
    HasTransactionSupportEnabled = true,
    TransactionOptions = transactionOptions
};
```

### Working with Collections
```csharp
public class DocumentService
{
    private readonly IMongoDbClientBase _mongoClient;

    public DocumentService(IMongoDbClientBase mongoClient)
    {
        _mongoClient = mongoClient;
    }

    public async Task<User> CreateUserAsync(User user)
    {
        var database = _mongoClient.GetDatabase();
        var collection = database.GetCollection<User>("users");
        
        await collection.InsertOneAsync(user);
        return user;
    }

    public async Task<List<User>> GetActiveUsersAsync()
    {
        var database = _mongoClient.GetDatabase();
        var collection = database.GetCollection<User>("users");
        
        return await collection
            .Find(u => u.IsActive)
            .ToListAsync();
    }
}
```

## Key Features

- **Pre-configured Serialization**: Automatic setup of BSON conventions and serializers
- **Transaction Support**: Optional MongoDB transaction capabilities
- **SSL/TLS Security**: Built-in TLS 1.2 support for secure connections
- **Flexible Configuration**: Comprehensive options for different MongoDB setups
- **Read Preferences**: Support for primary/secondary read preferences
- **Logging Integration**: Optional command text logging and diagnostics
- **Type Safety**: Strong typing with generic collection operations
- **Convention-based**: Automatic camel case and enum string representation
