# TestingCommons.Azure

The Azure project provides authentication utilities for Azure Active Directory integration in testing scenarios. It simplifies the process of obtaining access tokens using client credentials flow.

## Key Components

### **AzureAdAuthenticator**
- Handles Azure AD authentication using client credentials flow
- Automatically manages token expiration and renewal
- Caches tokens until they expire to avoid unnecessary requests
- Uses `IDateTimeProvider` for testable time-based operations

### **AzureAdOptions**
- Configuration class for Azure AD settings
- Includes: TenantId, ClientId, ClientSecret, Scope
- Automatically constructs the authority URL from TenantId

### **Dependency Injection Extensions**
- `AddAzureAdAuthenticator()` - Registers all required services
- Automatically configures options from configuration section "AzureAd"

## Usage Examples

### Configuration (appsettings.json)
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

### Service Registration
```csharp
// In Program.cs or Startup.cs
services.AddAzureAdAuthenticator(configuration);
```

### Using the Authenticator
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

### Manual Configuration
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

## Key Features

- **Token Caching**: Automatically caches tokens and only renews when expired
- **Testable**: Uses `IDateTimeProvider` for mockable time operations
- **Configuration-Driven**: Easy setup through appsettings.json
- **Client Credentials Flow**: Suitable for service-to-service authentication
- **Dependency Injection Ready**: Built with DI container in mind
