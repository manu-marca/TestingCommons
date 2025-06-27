using Microsoft.Extensions.Options;
using TestingCommons.Azure.Authenticator;

namespace TestingCommons.RestApiClient
{
    public class RestClientBase
    {
        protected readonly AzureAdAuthenticator? Authenticator;
        protected readonly HttpClient Client;

        protected RestClientBase(IOptions<IRestClientOptions> restClientOptions, AzureAdAuthenticator authenticator)
        {
            var options = restClientOptions.Value;
            Authenticator = authenticator;
            Client = new HttpClient
            {
                BaseAddress = new Uri(options.BaseUrl)
            };
        }

        protected RestClientBase(IOptions<IRestClientOptions> restClientOptions)
        {
            var options = restClientOptions.Value;
            Client = new HttpClient
            {
                BaseAddress = new Uri(options.BaseUrl)
            };
        }
    }
}
