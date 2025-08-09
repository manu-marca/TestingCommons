# TestingCommons.NewRelic

The NewRelic project provides a client for querying New Relic logs using GraphQL API. It enables automated testing scenarios that need to verify log entries and application behavior through New Relic monitoring data.

## Key Components

### **NewRelicClient**
- HTTP client wrapper for New Relic GraphQL API
- Supports log searching with customizable criteria
- Constructs NRQL (New Relic Query Language) queries automatically
- Returns raw HTTP responses for flexible result processing

### **NewRelicOptions**
- Configuration class for New Relic API settings
- Includes: BaseUrl, ApiKey, Account number
- Used for dependency injection configuration

### **NewRelicSearchCriteria**
- Defines search parameters for log queries
- Supports multiple result columns and search parameters
- Includes time range filtering (FromTime, ToTime)
- Enables case-insensitive searching across all columns

## Usage Examples

### Configuration
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

### Basic Log Search
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

### Integration Testing Example
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

## Key Features

- **GraphQL Integration**: Uses New Relic's GraphQL API for efficient querying
- **NRQL Generation**: Automatically constructs New Relic Query Language queries
- **Flexible Search**: Support for multiple columns and search parameters
- **Case-Insensitive**: Built-in case-insensitive search across all columns
- **Time Range Filtering**: Configurable time windows for log searches
- **HTTP Response Handling**: Returns raw HTTP responses for custom processing
- **Configuration-Driven**: Easy setup through dependency injection
- **Testing-Focused**: Designed specifically for automated testing scenarios

## Important Notes

- **API Key Security**: Ensure API keys are stored securely (Azure Key Vault, environment variables)
- **Rate Limiting**: Be aware of New Relic API rate limits in automated tests
- **Log Indexing Delay**: Allow time for logs to be indexed before querying
- **Account Access**: Ensure the API key has access to the specified account number
