using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TestingCommons.Azure.Options;
using TestingCommons.Azure.Authenticator;
using TestingCommons.Core.DateTimeProvider;
using TestingCommons.Core.Abstractions;

namespace TestingCommons.Azure.Extensions;
public static class IServiceCollectionExtensions
{
    public static IServiceCollection AddAzureAdAuthenticator(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<AzureAdOptions>(configuration.GetRequiredSection(AzureAdOptions.ConfigPath));
        services.AddTransient<IDateTimeProvider, UtcDateTimeProvider>();
        services.AddSingleton<AzureAdAuthenticator>();

        return services;
    }
}
