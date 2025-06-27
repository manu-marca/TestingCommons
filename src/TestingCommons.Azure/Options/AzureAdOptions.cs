namespace TestingCommons.Azure.Options;
public class AzureAdOptions
{
    public const string ConfigPath = "AzureAd";

    public required string TenantId { get; set; }

    public required string ClientId { get; set; }

    public required string ClientSecret { get; set; }

    public required string[] Scope { get; set; }

    public string Authority => $"https://login.microsoftonline.com/{TenantId}/oauth2/v2.0/token";
}
