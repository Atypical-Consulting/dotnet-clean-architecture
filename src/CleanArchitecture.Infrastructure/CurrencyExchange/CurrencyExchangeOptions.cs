namespace CleanArchitecture.Infrastructure.CurrencyExchange;

/// <summary>
///     Configuration options for the currency exchange service.
/// </summary>
public sealed class CurrencyExchangeOptions
{
    public const string SectionName = "CurrencyExchangeModule";

    /// <summary>
    ///     The base URL for the currency exchange API.
    /// </summary>
    public string ApiUrl { get; set; } = "https://api.frankfurter.app/latest?from=USD";

    /// <summary>
    ///     Optional API key for providers that require authentication.
    /// </summary>
    public string ApiKey { get; set; } = string.Empty;
}
