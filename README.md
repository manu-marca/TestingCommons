# Testing Commons Documentation

This document provides comprehensive documentation for all projects in the Testing Commons solution, including usage examples and key features for each component.

---

## Table of Contents

1. [TestingCommons.Core](#testingcommonscore)
2. [TestingCommons.Azure](#testingcommonsazure)
3. [TestingCommons.MongoDb](#testingcommonsmongodb)
4. [TestingCommons.NewRelic](#testingcommonsnewrelic)
5. [TestingCommons.Reqnroll](#testingcommonsreqnroll)
6. [TestingCommons.RestApiClient](#testingcommonsrestapiclient)
7. [TestingCommons.RabbitMq](#testingcommonsrabbitmq)
8. [TestingCommons.UnitTests](#testingcommonsunittests) *(Internal test project)*

---

## TestingCommons.Core

The Core project provides fundamental utilities, extensions, and abstractions used across testing scenarios. It includes common operations, date/time utilities, number extensions, and testable date-time providers.

### Key Components

#### **Utilities (`Utils` namespace)**

**CommonOperations**
- `GetMaskedIban(string iban)` - Masks IBAN numbers for secure display, showing only the last 4 characters

**DateTimeExtensions**
- `StripMilliseconds(DateTime)` - Removes milliseconds from DateTime for comparison purposes
- `StripTime(DateTime)` - Removes time component, keeping only the date
- `ParseDateTimeWithFormat(string, string)` - Parses DateTime strings with custom formats
- `ShiftTo1StDayOfNextMonth(DateTime)` - Moves date to the first day of the next month
- `ShiftTo1StDayOfCurrentMonth(DateTime)` - Moves date to the first day of current month
- `ShiftToLastDayOfCurrentMonth(DateTime)` - Moves date to the last day of current month

**NumberExtensions**
- `GetNegativeFromPositive(int/decimal/double)` - Converts positive numbers to negative (leaves negatives unchanged)

#### **DateTime Provider**

**UtcDateTimeProvider**
- Implements `IDateTimeProvider` for testable date/time operations
- Always returns UTC times to ensure consistency across time zones
- Properties: `Now` (DateTime.UtcNow), `NowOffset` (DateTimeOffset.UtcNow)

### Usage Examples

```csharp
// IBAN Masking
string iban = "DE1234567890123456";
string masked = CommonOperations.GetMaskedIban(iban);
// Result: "**************3456"

// DateTime Extensions
DateTime now = DateTime.Now;
DateTime dateOnly = now.StripTime();           // 2025-08-09 00:00:00
DateTime noMillis = now.StripMilliseconds();   // 2025-08-09 14:30:45
DateTime firstDay = now.ShiftTo1StDayOfCurrentMonth(); // 2025-08-01 14:30:45

// Number Extensions
decimal amount = 100.50m;
decimal negative = amount.GetNegativeFromPositive(); // -100.50

// Testable DateTime Provider
IDateTimeProvider dateProvider = new UtcDateTimeProvider();
DateTime utcNow = dateProvider.Now;
DateTimeOffset utcNowOffset = dateProvider.NowOffset;
```

### Key Features

- **IBAN Security**: Safe display of sensitive financial data
- **Date Manipulation**: Common date operations for testing scenarios
- **Number Utilities**: Consistent handling of positive/negative conversions
- **Testable Time**: Abstracted DateTime provider for unit testing
- **UTC Consistency**: Ensures all times are in UTC to avoid timezone issues

---

## TestingCommons.Azure

The Azure project provides authentication utilities for Azure Active Directory integration in testing scenarios. It simplifies the process of obtaining access tokens using client credentials flow.

### Key Components

#### **AzureAdAuthenticator**
- Handles Azure AD authentication using client credentials flow
- Automatically manages token expiration and renewal
- Caches tokens until they expire to avoid unnecessary requests
- Uses `IDateTimeProvider` for testable time-based operations

#### **AzureAdOptions**
- Configuration class for Azure AD settings
- Includes: TenantId, ClientId, ClientSecret, Scope
- Automatically constructs the authority URL from TenantId

#### **Dependency Injection Extensions**
- `AddAzureAdAuthenticator()` - Registers all required services
- Automatically configures options from configuration section "AzureAd"

### Usage Examples

#### Configuration (appsettings.json)
```json
{
  "AzureAd": {
    "TenantId": "your-tenant-id",
    "ClientId": "your-client-id", 
    "ClientSecret": "your-client-secret",
    "Scope": ["https://graph.microsoft.com/.default"]
  }
}
```

#### Service Registration
```csharp
// In Program.cs or Startup.cs
services.AddAzureAdAuthenticator(configuration);
```

#### Using the Authenticator
```csharp
public class MyTestService
{
    private readonly AzureAdAuthenticator _authenticator;

    public MyTestService(AzureAdAuthenticator authenticator)
    {
        _authenticator = authenticator;
    }

    public async Task<string> CallProtectedApiAsync()
    {
        // Get access token (automatically handles caching and renewal)
        string accessToken = await _authenticator.GetAccessTokenAsync();
        
        // Use token in HTTP requests
        using var client = new HttpClient();
        client.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
            
        var response = await client.GetAsync("https://api.example.com/protected");
        return await response.Content.ReadAsStringAsync();
    }
}
```

#### Manual Configuration
```csharp
// For scenarios where you need manual configuration
var options = Microsoft.Extensions.Options.Options.Create(new AzureAdOptions
{
    TenantId = "your-tenant-id",
    ClientId = "your-client-id",
    ClientSecret = "your-client-secret",
    Scope = new[] { "https://graph.microsoft.com/.default" }
});

var dateProvider = new UtcDateTimeProvider();
var authenticator = new AzureAdAuthenticator(options, dateProvider);

string token = await authenticator.GetAccessTokenAsync();
```

### Key Features

- **Token Caching**: Automatically caches tokens and only renews when expired
- **Testable**: Uses `IDateTimeProvider` for mockable time operations
- **Configuration-Driven**: Easy setup through appsettings.json
- **Client Credentials Flow**: Suitable for service-to-service authentication
- **Dependency Injection Ready**: Built with DI container in mind

---

## TestingCommons.MongoDb

The MongoDb project provides a robust base client for MongoDB operations in testing scenarios. It includes pre-configured serialization, transaction support, and comprehensive configuration options.

### Key Components

#### **MongoDbClientBase**
- Abstract base class for MongoDB client implementations
- Pre-configured with standard BSON serialization conventions
- Supports TLS 1.2 connections and SSL settings
- Automatic configuration of camel case conventions and enum string representation

#### **MongoDbConfiguration**
- Comprehensive configuration class for MongoDB settings
- Connection string, database name, logging options
- Transaction support configuration
- Read preference modes and staleness settings
- Message consume delay configuration

#### **Transaction Support**
- `MongoDbTransactionOptions` - Configuration for MongoDB transactions
- Read/Write concern level management
- Support for distributed transactions when enabled

#### **Serialization Provider**
- Custom BSON serialization for .NET types
- Decimal serialization as BSON Decimal128
- DateTimeOffset serialization as string
- Proper handling of nullable types

### Usage Examples

#### Basic Configuration
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

#### Creating a Custom Client
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

#### Transaction Configuration
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

#### Advanced Configuration with Read Preferences
```csharp
var config = new MongoDbConfiguration
{
    ConnectionString = "mongodb://primary:27017,secondary:27017",
    Database = "TestDatabase",
    ReadPreferenceMode = ReadPreferenceMode.SecondaryPreferred,
    MaxStalenessSeconds = 120, // Min 90 seconds
    MessageConsumeDelay = new MongoDbMessageConsumeDelayConfiguration
    {
        // Configure message consumption delays if needed
    }
};
```

#### Working with Collections
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

### Key Features

- **Pre-configured Serialization**: Automatic setup of BSON conventions and serializers
- **Transaction Support**: Optional MongoDB transaction capabilities
- **SSL/TLS Security**: Built-in TLS 1.2 support for secure connections
- **Flexible Configuration**: Comprehensive options for different MongoDB setups
- **Read Preferences**: Support for primary/secondary read preferences
- **Logging Integration**: Optional command text logging and diagnostics
- **Type Safety**: Strong typing with generic collection operations
- **Convention-based**: Automatic camel case and enum string representation

---

## TestingCommons.NewRelic

The NewRelic project provides a client for querying New Relic logs using GraphQL API. It enables automated testing scenarios that need to verify log entries and application behavior through New Relic monitoring data.

### Key Components

#### **NewRelicClient**
- HTTP client wrapper for New Relic GraphQL API
- Supports log searching with customizable criteria
- Constructs NRQL (New Relic Query Language) queries automatically
- Returns raw HTTP responses for flexible result processing

#### **NewRelicOptions**
- Configuration class for New Relic API settings
- Includes: BaseUrl, ApiKey, Account number
- Used for dependency injection configuration

#### **NewRelicSearchCriteria**
- Defines search parameters for log queries
- Supports multiple result columns and search parameters
- Includes time range filtering (FromTime, ToTime)
- Enables case-insensitive searching across all columns

### Usage Examples

#### Configuration
```csharp
// appsettings.json
{
  "NewRelic": {
    "BaseUrl": "https://api.newrelic.com/graphql",
    "ApiKey": "your-api-key-here",
    "Account": 12345678
  }
}

// Service registration
services.Configure<NewRelicOptions>(configuration.GetSection("NewRelic"));
services.AddTransient<NewRelicClient>();
```

#### Basic Log Search
```csharp
public class LogVerificationService
{
    private readonly NewRelicClient _newRelicClient;

    public LogVerificationService(NewRelicClient newRelicClient)
    {
        _newRelicClient = newRelicClient;
    }

    public async Task<bool> VerifyLogEntryExists(string searchTerm)
    {
        var criteria = new NewRelicSearchCriteria
        {
            ResultColumns = new[] { "timestamp", "message", "level" },
            SearchParameters = new[] { searchTerm },
            FromTime = DateTime.UtcNow.AddHours(-1),
            ToTime = DateTime.UtcNow
        };

        var response = _newRelicClient.SearchLog(criteria);
        
        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            // Parse GraphQL response to check for results
            return content.Contains("results") && !content.Contains("\"results\":[]");
        }

        return false;
    }
}
```

#### Advanced Search with Multiple Parameters
```csharp
public async Task<HttpResponseMessage> SearchErrorLogs(string applicationName, string errorType)
{
    var criteria = new NewRelicSearchCriteria
    {
        ResultColumns = new[] 
        { 
            "timestamp", 
            "message", 
            "level", 
            "app.name",
            "error.class",
            "error.message" 
        },
        SearchParameters = new[] 
        { 
            applicationName, 
            errorType,
            "ERROR" 
        },
        FromTime = DateTime.UtcNow.AddDays(-1),
        ToTime = DateTime.UtcNow
    };

    return _newRelicClient.SearchLog(criteria);
}
```

#### Integration Testing Example
```csharp
[Test]
public async Task ProcessOrder_ShouldLogSuccessMessage()
{
    // Arrange
    var order = new Order { Id = 123, Amount = 100.50m };
    var expectedLogMessage = $"Order {order.Id} processed successfully";

    // Act - Execute the business operation
    await _orderService.ProcessOrderAsync(order);

    // Wait for logs to be indexed in New Relic
    await Task.Delay(TimeSpan.FromSeconds(10));

    // Assert - Verify log entry exists
    var criteria = new NewRelicSearchCriteria
    {
        ResultColumns = new[] { "message", "timestamp", "app.name" },
        SearchParameters = new[] { expectedLogMessage, order.Id.ToString() },
        FromTime = DateTime.UtcNow.AddMinutes(-5),
        ToTime = DateTime.UtcNow
    };

    var response = _newRelicClient.SearchLog(criteria);
    response.Should().BeSuccessful();
    
    var content = await response.Content.ReadAsStringAsync();
    content.Should().Contain("processed successfully");
}
```

#### Custom Query Building
```csharp
public class CustomNewRelicQueries
{
    private readonly NewRelicClient _client;

    public CustomNewRelicQueries(NewRelicClient client)
    {
        _client = client;
    }

    public HttpResponseMessage SearchByLogLevel(string level, int hoursBack = 1)
    {
        return _client.SearchLog(new NewRelicSearchCriteria
        {
            ResultColumns = new[] { "timestamp", "message", "level", "app.name" },
            SearchParameters = new[] { level },
            FromTime = DateTime.UtcNow.AddHours(-hoursBack),
            ToTime = DateTime.UtcNow
        });
    }

    public HttpResponseMessage SearchApplicationLogs(string appName, params string[] searchTerms)
    {
        var parameters = new List<string> { appName };
        parameters.AddRange(searchTerms);

        return _client.SearchLog(new NewRelicSearchCriteria
        {
            ResultColumns = new[] { "timestamp", "message", "level", "app.name", "host" },
            SearchParameters = parameters.ToArray(),
            FromTime = DateTime.UtcNow.AddHours(-2),
            ToTime = DateTime.UtcNow
        });
    }
}
```

### Key Features

- **GraphQL Integration**: Uses New Relic's GraphQL API for efficient querying
- **NRQL Generation**: Automatically constructs New Relic Query Language queries
- **Flexible Search**: Support for multiple columns and search parameters
- **Case-Insensitive**: Built-in case-insensitive search across all columns
- **Time Range Filtering**: Configurable time windows for log searches
- **HTTP Response Handling**: Returns raw HTTP responses for custom processing
- **Configuration-Driven**: Easy setup through dependency injection
- **Testing-Focused**: Designed specifically for automated testing scenarios

### Important Notes

- **API Key Security**: Ensure API keys are stored securely (Azure Key Vault, environment variables)
- **Rate Limiting**: Be aware of New Relic API rate limits in automated tests
- **Log Indexing Delay**: Allow time for logs to be indexed before querying
- **Account Access**: Ensure the API key has access to the specified account number

---

## TestingCommons.Reqnroll

The Reqnroll project provides extensions and utilities for behavior-driven development (BDD) testing using Reqnroll (formerly SpecFlow). It includes data table helpers and relative date/time parsing for more expressive test scenarios.

### Key Components

#### **DataTableExtensions**
- `GetVerticalTableData()` - Converts Reqnroll DataTable to Dictionary<string, string>
- `GetValueFromVerticalTableByName()` - Extracts specific values from vertical data tables
- Case-insensitive key lookup for flexible test data access

#### **RelativeDateValueRetriever**
- Custom value retriever for Reqnroll.Assist that handles relative dates
- Supports natural language date expressions in test scenarios
- Configurable test moment for consistent test execution
- Handles both past and future relative dates

### Usage Examples

#### Data Table Extensions

**Feature File Example:**
```gherkin
Scenario: User registration with vertical data table
    Given I have the following user data:
      | Field     | Value              |
      | Name      | John Doe           |
      | Email     | john@example.com   |
      | Age       | 30                 |
      | IsActive  | true               |
    When I register the user
    Then the user should be created successfully
```

**Step Definition:**
```csharp
[Given(@"I have the following user data:")]
public void GivenIHaveTheFollowingUserData(DataTable table)
{
    // Convert to dictionary for easy access
    var userData = table.GetVerticalTableData();
    
    var user = new User
    {
        Name = userData["Name"],
        Email = userData["Email"],
        Age = int.Parse(userData["Age"]),
        IsActive = bool.Parse(userData["IsActive"])
    };
    
    _scenarioContext["User"] = user;
}

[Then(@"the (.*) field should be (.*)")]
public void ThenTheFieldShouldBe(string fieldName, string expectedValue, DataTable table)
{
    // Get specific value from table
    var actualValue = table.GetValueFromVerticalTableByName(fieldName);
    actualValue.Should().Be(expectedValue);
}
```

#### Relative Date Value Retriever

**Setup in Test Hooks:**
```csharp
[BeforeTestRun]
public static void BeforeTestRun()
{
    // Set a fixed test moment for consistent date calculations
    RelativeDateValueRetriever.TestMoment = new DateTime(2025, 8, 9, 14, 30, 0);
    
    // Register the custom value retriever
    Service.Instance.ValueRetrievers.Register(new RelativeDateValueRetriever());
}
```

**Feature File with Relative Dates:**
```gherkin
Scenario: Order processing with relative dates
    Given I have an order with the following details:
      | Field        | Value           |
      | OrderDate    | 2 days ago      |
      | DeliveryDate | in 5 days       |
      | CreatedAt    | 1 hour ago      |
      | UpdatedAt    | now             |
      | CancelledAt  | null date       |
    When I process the order
    Then the dates should be calculated correctly
```

**Step Definition with Object Creation:**
```csharp
[Given(@"I have an order with the following details:")]
public void GivenIHaveAnOrderWithTheFollowingDetails(DataTable table)
{
    // Reqnroll.Assist will automatically use RelativeDateValueRetriever
    var order = table.CreateInstance<Order>();
    
    // The dates will be automatically calculated:
    // OrderDate = TestMoment.AddDays(-2)
    // DeliveryDate = TestMoment.AddDays(5)
    // CreatedAt = TestMoment.AddHours(-1)
    // UpdatedAt = TestMoment
    // CancelledAt = null
    
    _scenarioContext["Order"] = order;
}

public class Order
{
    public DateTime OrderDate { get; set; }
    public DateTime DeliveryDate { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime? CancelledAt { get; set; }
}
```

#### Advanced Relative Date Examples

**Supported Date Expressions:**
```gherkin
Scenario Outline: Various relative date formats
    Given the date expression "<expression>" 
    Then it should resolve to the expected date

    Examples:
      | expression      | description                    |
      | now             | Current test moment            |
      | 1 day ago       | Yesterday                      |
      | 2 weeks ago     | Two weeks in the past          |
      | in 3 months     | Three months in the future     |
      | 5 years ago     | Five years in the past         |
      | in 30 minutes   | Thirty minutes in the future   |
      | 45 seconds ago  | Forty-five seconds in the past |
      | null date       | Null DateTime value            |
```

#### Combining with Other Reqnroll Features

**Complex Scenario with Mixed Data:**
```csharp
[Given(@"I have the following events:")]
public void GivenIHaveTheFollowingEvents(DataTable table)
{
    var events = table.CreateSet<Event>().ToList();
    
    foreach (var evt in events)
    {
        // All relative dates are automatically resolved
        Console.WriteLine($"Event: {evt.Name} at {evt.ScheduledDate}");
    }
    
    _scenarioContext["Events"] = events;
}

public class Event
{
    public string Name { get; set; }
    public DateTime ScheduledDate { get; set; }
    public DateTime? CompletedDate { get; set; }
    public bool IsActive { get; set; }
}
```

**Feature File:**
```gherkin
Given I have the following events:
  | Name           | ScheduledDate | CompletedDate | IsActive |
  | Morning Standup | 2 hours ago   | 2 hours ago   | false    |
  | Code Review     | 1 hour ago    | null date     | true     |
  | Deployment      | in 30 minutes | null date     | true     |
```

### Key Features

- **Natural Language Dates**: Express dates in human-readable format in test scenarios
- **Consistent Test Execution**: Fixed test moment ensures reproducible test results
- **Flexible Data Access**: Easy conversion between Reqnroll DataTables and .NET objects
- **Case-Insensitive Lookup**: Robust data table value retrieval
- **Null Date Support**: Proper handling of optional date fields
- **Multiple Time Units**: Support for years, months, days, hours, minutes, seconds
- **Past and Future**: Handle both historical and future relative dates
- **Reqnroll.Assist Integration**: Seamless integration with object creation from tables

### Supported Date Formats

- **Past Dates**: `1 day ago`, `2 weeks ago`, `3 months ago`, `5 years ago`
- **Future Dates**: `in 1 day`, `in 2 weeks`, `in 3 months`, `in 5 years`
- **Current Time**: `now`
- **Null Values**: `null date`
- **Time Units**: `second(s)`, `minute(s)`, `hour(s)`, `day(s)`, `month(s)`, `year(s)`

---

## TestingCommons.RestApiClient

The RestApiClient project provides a base class for creating HTTP clients with optional Azure AD authentication. It simplifies the creation of REST API clients for testing scenarios by handling common configuration and authentication patterns.

### Key Components

#### **RestClientBase**
- Abstract base class for REST API clients
- Pre-configured HttpClient with base URL
- Optional Azure AD authentication integration
- Two constructor overloads: with and without authentication

#### **IRestClientOptions**
- Interface defining configuration options for REST clients
- Contains BaseUrl property for API endpoint configuration
- Designed for dependency injection and configuration binding

### Usage Examples

#### Basic REST Client Without Authentication

**Configuration:**
```csharp
// appsettings.json
{
  "ApiSettings": {
    "BaseUrl": "https://api.example.com/"
  }
}

// Options class
public class ApiOptions : IRestClientOptions
{
    public string BaseUrl { get; set; }
}

// Service registration
services.Configure<ApiOptions>(configuration.GetSection("ApiSettings"));
services.AddTransient<MyApiClient>();
```

**Client Implementation:**
```csharp
public class MyApiClient : RestClientBase
{
    public MyApiClient(IOptions<ApiOptions> options) : base(options)
    {
    }

    public async Task<User> GetUserAsync(int userId)
    {
        var response = await Client.GetAsync($"users/{userId}");
        response.EnsureSuccessStatusCode();
        
        var json = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<User>(json);
    }

    public async Task<User> CreateUserAsync(User user)
    {
        var json = JsonSerializer.Serialize(user);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        
        var response = await Client.PostAsync("users", content);
        response.EnsureSuccessStatusCode();
        
        var responseJson = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<User>(responseJson);
    }

    public async Task<bool> DeleteUserAsync(int userId)
    {
        var response = await Client.DeleteAsync($"users/{userId}");
        return response.IsSuccessStatusCode;
    }
}
```

#### REST Client with Azure AD Authentication

**Configuration:**
```csharp
// appsettings.json
{
  "ApiSettings": {
    "BaseUrl": "https://secure-api.example.com/"
  },
  "AzureAd": {
    "TenantId": "your-tenant-id",
    "ClientId": "your-client-id",
    "ClientSecret": "your-client-secret",
    "Scope": ["https://secure-api.example.com/.default"]
  }
}

// Service registration
services.Configure<ApiOptions>(configuration.GetSection("ApiSettings"));
services.AddAzureAdAuthenticator(configuration);
services.AddTransient<SecureApiClient>();
```

**Authenticated Client Implementation:**
```csharp
public class SecureApiClient : RestClientBase
{
    public SecureApiClient(IOptions<ApiOptions> options, AzureAdAuthenticator authenticator) 
        : base(options, authenticator)
    {
    }

    public async Task<Order> GetOrderAsync(int orderId)
    {
        // Add authentication header
        await AddAuthenticationHeaderAsync();
        
        var response = await Client.GetAsync($"orders/{orderId}");
        response.EnsureSuccessStatusCode();
        
        var json = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<Order>(json);
    }

    public async Task<List<Order>> GetUserOrdersAsync(int userId)
    {
        await AddAuthenticationHeaderAsync();
        
        var response = await Client.GetAsync($"users/{userId}/orders");
        response.EnsureSuccessStatusCode();
        
        var json = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<List<Order>>(json);
    }

    private async Task AddAuthenticationHeaderAsync()
    {
        if (Authenticator != null)
        {
            var token = await Authenticator.GetAccessTokenAsync();
            Client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        }
    }
}
```

#### Advanced Client with Custom Headers and Error Handling

```csharp
public class EnterpriseApiClient : RestClientBase
{
    private readonly ILogger<EnterpriseApiClient> _logger;

    public EnterpriseApiClient(
        IOptions<ApiOptions> options, 
        AzureAdAuthenticator authenticator,
        ILogger<EnterpriseApiClient> logger) : base(options, authenticator)
    {
        _logger = logger;
        
        // Configure default headers
        Client.DefaultRequestHeaders.Add("User-Agent", "TestingCommons/1.0");
        Client.DefaultRequestHeaders.Add("Accept", "application/json");
        Client.Timeout = TimeSpan.FromMinutes(5);
    }

    public async Task<T> GetAsync<T>(string endpoint)
    {
        try
        {
            await AddAuthenticationHeaderAsync();
            
            _logger.LogInformation("Calling GET {Endpoint}", endpoint);
            var response = await Client.GetAsync(endpoint);
            
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("API call failed: {StatusCode} - {Content}", 
                    response.StatusCode, errorContent);
                throw new HttpRequestException($"API call failed: {response.StatusCode}");
            }
            
            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<T>(json);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling API endpoint {Endpoint}", endpoint);
            throw;
        }
    }

    public async Task<TResponse> PostAsync<TRequest, TResponse>(string endpoint, TRequest data)
    {
        try
        {
            await AddAuthenticationHeaderAsync();
            
            var json = JsonSerializer.Serialize(data);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            
            _logger.LogInformation("Calling POST {Endpoint}", endpoint);
            var response = await Client.PostAsync(endpoint, content);
            
            response.EnsureSuccessStatusCode();
            
            var responseJson = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<TResponse>(responseJson);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error posting to API endpoint {Endpoint}", endpoint);
            throw;
        }
    }

    private async Task AddAuthenticationHeaderAsync()
    {
        if (Authenticator != null)
        {
            var token = await Authenticator.GetAccessTokenAsync();
            Client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        }
    }
}
```

#### Testing with the REST Client

```csharp
[Test]
public async Task GetUser_ShouldReturnUserDetails()
{
    // Arrange
    var userId = 123;
    
    // Act
    var user = await _apiClient.GetUserAsync(userId);
    
    // Assert
    user.Should().NotBeNull();
    user.Id.Should().Be(userId);
    user.Name.Should().NotBeNullOrEmpty();
}

[Test]
public async Task CreateUser_ShouldReturnCreatedUser()
{
    // Arrange
    var newUser = new User
    {
        Name = "John Doe",
        Email = "john@example.com"
    };
    
    // Act
    var createdUser = await _apiClient.CreateUserAsync(newUser);
    
    // Assert
    createdUser.Should().NotBeNull();
    createdUser.Id.Should().BeGreaterThan(0);
    createdUser.Name.Should().Be(newUser.Name);
    createdUser.Email.Should().Be(newUser.Email);
}
```

### Key Features

- **Base URL Configuration**: Automatic base URL setup for consistent API calls
- **Optional Authentication**: Support for Azure AD authentication when needed
- **HttpClient Management**: Pre-configured HttpClient with proper disposal
- **Flexible Construction**: Two constructor overloads for different scenarios
- **Configuration-Driven**: Easy setup through dependency injection
- **Extensible Design**: Abstract base class allows for custom implementations
- **Authentication Integration**: Seamless integration with Azure AD authenticator
- **Testing-Friendly**: Designed specifically for testing scenarios

### Design Patterns

- **Template Method**: Base class provides structure, derived classes implement specifics
- **Dependency Injection**: Full support for .NET DI container
- **Options Pattern**: Configuration through strongly-typed options classes
- **Factory Pattern**: HttpClient creation and configuration handled in base class

---

## TestingCommons.RabbitMq

The RabbitMq project provides a comprehensive client for RabbitMQ message broker operations. It supports both message publishing and consumption with flexible configuration options, making it ideal for testing message-driven architectures.

### Key Components

#### **RabbitMqClient**
- Complete message broker client supporting publish and consume operations
- Multiple constructor overloads for different configuration scenarios
- Automatic connection health monitoring and startup
- Built-in consumer management and graceful shutdown

#### **RabbitMqConfig**
- Comprehensive configuration for RabbitMQ connections
- Support for clusters, TLS connections, and authentication
- Configurable heartbeat intervals and redelivery settings

#### **BusControl**
- Factory methods for creating MassTransit bus instances
- Support for advanced receive endpoint configurations
- Extensions for RabbitMQ-specific settings

#### **Message Builders**
- Pre-built message builders for common message types
- Consistent message structure for testing scenarios

### Usage Examples

#### Basic Configuration
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

#### Advanced Configuration with TLS and Clustering
```csharp
var config = new RabbitMqConfig
{
    Hostname = "rabbitmq-cluster.example.com",
    Username = "test-user",
    Password = "secure-password",
    VirtualHost = "/test",
    IsTlsConnection = true,
    UseTlsPolicy = true,
    ClusterNodes = "node1.example.com,node2.example.com,node3.example.com",
    HeartbeatInterval = 30,
    Redelivery = new RedeliverySettings
    {
        // Configure redelivery settings
    }
};

using var client = new RabbitMqClient(config);
```

#### Publishing Messages
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

## Message Consumption

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

### Custom Consumer Class
```csharp
public class MyMessageConsumer : IConsumer<MyMessage>
{
    public async Task Consume(ConsumeContext<MyMessage> context)
    {
        var message = context.Message;
        Console.WriteLine($"Processing message: {message.Content}");
        
        // Your message processing logic here
        await ProcessMessageAsync(message);
    }
    
    private async Task ProcessMessageAsync(MyMessage message)
    {
        // Implementation here
        await Task.Delay(100); // Simulate processing
    }
}

// Usage
var consumer = new MyMessageConsumer();
var handle = client.ConsumeMessages("my-queue", consumer);
```

## Advanced Configuration with Receive Endpoints

```csharp
// Configure receive endpoints with specific settings
var receiveEndpointConfigurations = new Dictionary<string, Action<IRabbitMqReceiveEndpointConfigurator>>
{
    ["high-priority-queue"] = endpoint =>
    {
        endpoint.PrefetchCount = 1; // Process one message at a time
        endpoint.UseConcurrencyLimit(1);
        endpoint.Consumer<HighPriorityMessageConsumer>();
    },
    ["batch-processing-queue"] = endpoint =>
    {
        endpoint.PrefetchCount = 50; // Process multiple messages
        endpoint.UseConcurrencyLimit(10);
        endpoint.Consumer<BatchProcessingConsumer>();
    }
};

using var client = new RabbitMqClient(config, receiveEndpointConfigurations);

// The consumers are automatically started with the client
await client.WaitForMessages(TimeSpan.FromHours(1));
```

## Message Publishing and Consuming in the Same Client

```csharp
using var client = new RabbitMqClient(config);

// Set up a consumer
var handle = client.ConsumeMessages<OrderCreated>("order-events", async order =>
{
    Console.WriteLine($"Processing order: {order.OrderId}");
    // Process the order
});

// Publish messages
for (int i = 0; i < 10; i++)
{
    var order = new OrderCreated { OrderId = i, Amount = 100.0m };
    var requestId = client.PublishMessage(order);
    Console.WriteLine($"Published order {i} with request ID: {requestId}");
}

// Wait for messages to be processed
await client.WaitForMessages(TimeSpan.FromSeconds(30));
```

## Error Handling and Retry Logic

```csharp
await client.ConsumeMessages<RiskyMessage>("risky-queue", async (message, context) =>
{
    try
    {
        await ProcessRiskyMessage(message);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error processing message: {ex.Message}");
        
        // Check retry count from headers
        var retryCount = context.Headers.Get<int>("RetryCount") ?? 0;
        
        if (retryCount < 3)
        {
            // Republish with retry count
            client.PublishMessage(message, retryCount + 1);
        }
        else
        {
            // Send to dead letter queue or log for manual intervention
            Console.WriteLine($"Message failed after {retryCount} retries");
        }
    }
});
```

## Graceful Shutdown

```csharp
using var client = new RabbitMqClient(config);

// Set up consumers
var handle = client.ConsumeMessages<MyMessage>("my-queue", ProcessMessage);

// Handle cancellation (e.g., from console app cancellation token)
var cts = new CancellationTokenSource();
Console.CancelKeyPress += (s, e) =>
{
    e.Cancel = true;
    cts.Cancel();
};

try
{
    // Keep running until cancelled
    await Task.Delay(Timeout.Infinite, cts.Token);
}
catch (OperationCanceledException)
{
    Console.WriteLine("Shutting down...");
}

// Stop consumers gracefully
await client.StopConsumers();

// Client will be disposed automatically with 'using' statement
```

## Consumer Handle Management

The `ConsumeMessages` methods return a `HostReceiveEndpointHandle` that represents the connection to the message consumer. This handle can be used to manage individual consumers:

```csharp
// Start multiple consumers and track their handles
var orderHandle = client.ConsumeMessages<OrderCreated>("orders", ProcessOrder);
var paymentHandle = client.ConsumeMessages<PaymentReceived>("payments", ProcessPayment);
var userHandle = client.ConsumeMessages<UserRegistered>("users", ProcessUser);

// Stop a specific consumer
await orderHandle.StopAsync();

// Continue with other consumers running
await Task.Delay(TimeSpan.FromMinutes(5));

// Stop remaining consumers
await paymentHandle.StopAsync();
await userHandle.StopAsync();

// Or stop all consumers at once
await client.StopConsumers();
```

## Async Consumer Management

```csharp
public class MessageProcessor
{
    private readonly RabbitMqClient _client;
    private readonly List<HostReceiveEndpointHandle> _consumerHandles = new();

    public MessageProcessor(RabbitMqClient client)
    {
        _client = client;
    }

    public void StartConsumers()
    {
        var orderHandle = _client.ConsumeMessages<OrderMessage>("orders", ProcessOrder);
        var inventoryHandle = _client.ConsumeMessages<InventoryMessage>("inventory", ProcessInventory);
        
        _consumerHandles.AddRange(new[] { orderHandle, inventoryHandle });
    }

    public async Task StopAllConsumersAsync()
    {
        foreach (var handle in _consumerHandles)
        {
            await handle.StopAsync();
        }
        _consumerHandles.Clear();
    }

    private async Task ProcessOrder(OrderMessage order)
    {
        // Process order logic
        Console.WriteLine($"Processing order: {order.Id}");
    }

    private async Task ProcessInventory(InventoryMessage inventory)
    {
        // Process inventory logic
        Console.WriteLine($"Processing inventory: {inventory.ProductId}");
    }
}

## Example Message Classes

```csharp
public class MyMessage
{
    public string Content { get; set; }
    public DateTime Timestamp { get; set; }
}

public class OrderCreated
{
    public int OrderId { get; set; }
    public decimal Amount { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class RiskyMessage
{
    public string Data { get; set; }
    public bool ShouldFail { get; set; }
}
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

### Method Return Types

- **`ConsumeMessages<T>()` methods**: Return `HostReceiveEndpointHandle` for individual consumer management
- **`PublishMessage<T>()` method**: Returns `Guid` request ID for message tracking
- **`WaitForMessages()` method**: Returns `Task` for awaiting message processing
- **`StopConsumers()` method**: Returns `Task` for graceful shutdown of all consumers
- **`HostReceiveEndpointHandle.StopAsync()` method**: Async method for graceful consumer shutdown

---

## TestingCommons.UnitTests

The UnitTests project contains internal unit tests for validating the functionality of the other Testing Commons components. This project is not intended for external consumption but serves as a reference for testing patterns and validation of the library components.

### What It Tests

- **Core Utilities**: Tests for CommonOperations, DateTimeExtensions, NumberExtensions
- **IBAN Masking**: Validation of financial data security features
- **Date Manipulation**: Testing of date utility functions
- **Number Operations**: Verification of number extension methods

### Example Test Patterns

The tests demonstrate proper usage patterns and expected behaviors:

```csharp
[Test]
public void IbanWasMasked()
{
    var iban = "DE89370400440532013000";
    var ibanMasked = CommonOperations.GetMaskedIban(iban);
    Assert.That("******************3000", Is.EqualTo(ibanMasked));
}
```

This project serves as both validation and documentation of expected behavior for the Testing Commons components.

---

## Summary

The Testing Commons solution provides a comprehensive set of tools for building robust, testable applications with support for:

- **Core Utilities** for common operations and data manipulation
- **Azure Integration** for secure authentication and cloud services
- **Database Operations** with MongoDB client and configuration
- **Monitoring Integration** through New Relic log searching
- **BDD Testing** with Reqnroll extensions and relative date handling
- **REST API Communication** with optional authentication
- **Message Broker Operations** for RabbitMQ publishing and consumption

Each component is designed to work independently or in combination, providing maximum flexibility for different testing scenarios and architectural patterns.
