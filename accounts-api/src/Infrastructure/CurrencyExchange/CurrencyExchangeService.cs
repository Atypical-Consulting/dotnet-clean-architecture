namespace Infrastructure.CurrencyExchange;

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Application.Services;
using Domain.ValueObjects;

/// <summary>
///     Real implementation of the Exchange Service using external data source
/// </summary>
public sealed class CurrencyExchangeService : ICurrencyExchange
{
    public const string HttpClientName = "Fixer";

    [SuppressMessage("Minor Code Smell", "S1075:URIs should not be hardcoded", Justification = "<Pending>")]
    private const string _exchangeUrl = "https://api.exchangeratesapi.io/latest?base=USD";

    private readonly IHttpClientFactory _httpClientFactory;

    private readonly Dictionary<Currency, decimal> _usdRates = new Dictionary<Currency, decimal>();

    public CurrencyExchangeService(IHttpClientFactory httpClientFactory) =>
        this._httpClientFactory = httpClientFactory;

    /// <summary>
    ///     Converts allowed currencies into USD.
    /// </summary>
    /// <returns>Money.</returns>
    public async Task<Money> Convert(Money originalAmount, Currency destinationCurrency)
    {
        HttpClient httpClient = this._httpClientFactory.CreateClient(HttpClientName);
        Uri requestUri = new Uri(_exchangeUrl);

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
