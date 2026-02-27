namespace CleanArchitecture.ComponentTests.V2;

using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit;

public sealed class SunnyDayTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;
    public SunnyDayTests(CustomWebApplicationFactory factory) => this._factory = factory;

    private async Task<Tuple<Guid, decimal>> GetAccounts()
    {
        HttpClient client = this._factory.CreateClient();
        HttpResponseMessage actualResponse = await client
            .GetAsync("/api/v1/Accounts/")
            ;

        string actualResponseString = await actualResponse.Content
            .ReadAsStringAsync()
            ;

        using JsonDocument jsonDocument = JsonDocument.Parse(actualResponseString);
        JsonElement jsonResponse = jsonDocument.RootElement;

        Guid.TryParse(jsonResponse.GetProperty("accounts")[0].GetProperty("accountId").GetString(), out Guid accountId);
        decimal.TryParse(jsonResponse.GetProperty("accounts")[0].GetProperty("currentBalance").GetRawText(),
            out decimal currentBalance);

        return new Tuple<Guid, decimal>(accountId, currentBalance);
    }

    private async Task GetAccount(string accountId)
    {
        HttpClient client = this._factory.CreateClient();
        await client.GetAsync($"/api/v2/Accounts/{accountId}")
            ;
    }

    [Fact]
    public async Task GetAccounts_GetAccount()
    {
        Tuple<Guid, decimal> account = await this.GetAccounts()
            ;
        await this.GetAccount(account.Item1.ToString())
            ;
    }
}
