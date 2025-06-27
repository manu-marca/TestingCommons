using Microsoft.Extensions.Options;
using Microsoft.Identity.Client;
using TestingCommons.Azure.Options;
using TestingCommons.Core.Abstractions;

namespace TestingCommons.Azure.Authenticator;
public class AzureAdAuthenticator
{
    private IConfidentialClientApplication? _confidentialClientApplication;
    private readonly AzureAdOptions _options;
    private readonly IDateTimeProvider _dateTimeProvider;
    private AuthenticationResult? _result;

    public AzureAdAuthenticator(IOptions<AzureAdOptions> options, IDateTimeProvider dateTimeProvider)
    {
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
        _dateTimeProvider = dateTimeProvider ?? throw new ArgumentNullException(nameof(dateTimeProvider));
    }

    public virtual void CreateClient()
    {
        _confidentialClientApplication = ConfidentialClientApplicationBuilder.Create(_options.ClientId)
            .WithClientSecret(_options.ClientSecret)
            .WithAuthority(_options.Authority)
            .Build();
    }

    public virtual async Task<string> GetAccessTokenAsync()
    {
        if (_confidentialClientApplication is null)
        {
            CreateClient();
        }

        if (_result is null || _result.ExpiresOn < _dateTimeProvider.NowOffset)
        {
            _result = await _confidentialClientApplication.AcquireTokenForClient(_options.Scope).ExecuteAsync();
        }

        return _result.AccessToken;
    }
}
