namespace CleanArchitecture.Infrastructure.CurrencyExchange;

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using CleanArchitecture.Application.Services;
using CleanArchitecture.Domain.ValueObjects;
using Microsoft.Extensions.Options;

/// <summary>
///     Real implementation of the Exchange Service using external data source
/// </summary>
public sealed class CurrencyExchangeService : ICurrencyExchange
{
    public const string HttpClientName = "CurrencyExchange";

    private readonly IHttpClientFactory _httpClientFactory;
    private readonly CurrencyExchangeOptions _options;

    private readonly Dictionary<Currency, decimal> _usdRates = new Dictionary<Currency, decimal>();

    public CurrencyExchangeService(
        IHttpClientFactory httpClientFactory,
        IOptions<CurrencyExchangeOptions> options)
    {
        this._httpClientFactory = httpClientFactory;
        this._options = options.Value;
    }

    /// <summary>
    ///     Converts allowed currencies into USD.
    /// </summary>
    /// <returns>Money.</returns>
    public async Task<Money> Convert(Money originalAmount, Currency destinationCurrency)
    {
        HttpClient httpClient = this._httpClientFactory.CreateClient(HttpClientName);

        string url = this._options.ApiUrl;
        if (!string.IsNullOrEmpty(this._options.ApiKey))
        {
            string separator = url.Contains("?") ? "&" : "?";
            url = $"{url}{separator}apikey={this._options.ApiKey}";
        }

        Uri requestUri = new Uri(url);

        HttpResponseMessage response = await httpClient.GetAsync(requestUri)
            .ConfigureAwait(false);

        response.EnsureSuccessStatusCode();

        string responseJson = await response
            .Content
            .ReadAsStringAsync()
            .ConfigureAwait(false);

        this.ParseCurrencies(responseJson);

        decimal usdAmount = this._usdRates[originalAmount.Currency] / originalAmount.Amount;
        decimal destinationAmount = this._usdRates[destinationCurrency] / usdAmount;

        return new Money(
            destinationAmount,
            destinationCurrency);
    }

    private void ParseCurrencies(string responseJson)
    {
        using JsonDocument document = JsonDocument.Parse(responseJson);
        JsonElement rates = document.RootElement.GetProperty("rates");
        decimal eur = rates.GetProperty(Currency.Euro.Code).GetDecimal();
        decimal cad = rates.GetProperty(Currency.Canadian.Code).GetDecimal();
        decimal gbh = rates.GetProperty(Currency.BritishPound.Code).GetDecimal();
        decimal sek = rates.GetProperty(Currency.Krona.Code).GetDecimal();
        decimal brl = rates.GetProperty(Currency.Real.Code).GetDecimal();

        this._usdRates.Add(Currency.Dollar, 1);
        this._usdRates.Add(Currency.Euro, eur);
        this._usdRates.Add(Currency.Canadian, cad);
        this._usdRates.Add(Currency.BritishPound, gbh);
        this._usdRates.Add(Currency.Krona, sek);
        this._usdRates.Add(Currency.Real, brl);
    }
}
