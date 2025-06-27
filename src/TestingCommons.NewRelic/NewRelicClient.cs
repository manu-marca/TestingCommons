using Microsoft.Extensions.Options;

namespace TestingCommons.NewRelic
{
    public class NewRelicClient
    {
        protected readonly HttpClient client;

        private readonly NewRelicOptions _options;

        public NewRelicClient(IOptions<NewRelicOptions> newRelicOptions)
        {
            _options = newRelicOptions.Value;

            client = new() {};
        }

        public HttpResponseMessage SearchLog(NewRelicSearchCriteria searchCriteria)
        {
            var resultColumns = string.Join(" , ", searchCriteria.ResultColumns
                .Select(s => "`" + s + "`").ToArray());
            var searchFilters = string.Join(" , ", searchCriteria.SearchParameters
                .Select(s => "allColumnSearch('" + s + "', insensitive: true)").ToArray());
           
            var query = $"{{\"query\":\"{{ actor {{ nrql( query: \\\"SELECT {resultColumns} FROM Log WHERE {searchFilters} SINCE 1 hours ago\\\" accounts: {_options.Account} ) {{ results }}  }}}}\",\"variables\":{{}}}}";
            var content = new StringContent(query, null, "application/json");
            
            var request = new HttpRequestMessage(HttpMethod.Post, _options.BaseUrl)
            {
                Content = content,
            };
            request.Headers.Add("Api-Key", _options.ApiKey);

            return client.SendAsync(request).Result;
        }


    }
}
