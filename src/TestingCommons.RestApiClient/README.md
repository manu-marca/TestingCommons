# TestingCommons.RestApiClient

The RestApiClient project provides a base class for creating HTTP clients with optional Azure AD authentication. It simplifies the creation of REST API clients for testing scenarios by handling common configuration and authentication patterns.

## Key Components

### **RestClientBase**
- Abstract base class for REST API clients
- Pre-configured HttpClient with base URL
- Optional Azure AD authentication integration
- Two constructor overloads: with and without authentication

### **IRestClientOptions**
- Interface defining configuration options for REST clients
- Contains BaseUrl property for API endpoint configuration
- Designed for dependency injection and configuration binding

## Usage Examples

### Basic REST Client Without Authentication

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
}
```

### REST Client with Azure AD Authentication

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

## Key Features

- **Base URL Configuration**: Automatic base URL setup for consistent API calls
- **Optional Authentication**: Support for Azure AD authentication when needed
- **HttpClient Management**: Pre-configured HttpClient with proper disposal
- **Flexible Construction**: Two constructor overloads for different scenarios
- **Configuration-Driven**: Easy setup through dependency injection
- **Extensible Design**: Abstract base class allows for custom implementations
- **Authentication Integration**: Seamless integration with Azure AD authenticator
- **Testing-Friendly**: Designed specifically for testing scenarios

## Design Patterns

- **Template Method**: Base class provides structure, derived classes implement specifics
- **Dependency Injection**: Full support for .NET DI container
- **Options Pattern**: Configuration through strongly-typed options classes
- **Factory Pattern**: HttpClient creation and configuration handled in base class
