namespace ComponentTests.V1;

using System;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit;

[Collection("WebApi Collection")]
public sealed class GetAccountsTests
{
    private readonly CustomWebApplicationFactoryFixture _fixture;
    public GetAccountsTests(CustomWebApplicationFactoryFixture fixture) => this._fixture = fixture;

    [Fact]
    public async Task GetAccountsReturnsList()
    {
        HttpClient client = this._fixture
            .CustomWebApplicationFactory
            .CreateClient();

        HttpResponseMessage actualResponse = await client
            .GetAsync("/api/v1/Accounts/")
            ;

        string actualResponseString = await actualResponse.Content
            .ReadAsStringAsync()
            ;

        Assert.Equal(HttpStatusCode.OK, actualResponse.StatusCode);

        using JsonDocument jsonDocument = JsonDocument.Parse(actualResponseString);
        JsonElement jsonResponse = jsonDocument.RootElement;

        JsonElement accountIdElement = jsonResponse.GetProperty("accounts")[0].GetProperty("accountId");
        JsonElement currentBalanceElement = jsonResponse.GetProperty("accounts")[0].GetProperty("currentBalance");

        Assert.Equal(JsonValueKind.String, accountIdElement.ValueKind);
        Assert.Equal(JsonValueKind.Number, currentBalanceElement.ValueKind);

        Assert.True(Guid.TryParse(accountIdElement.GetString(), out Guid _));
        Assert.True(decimal.TryParse(currentBalanceElement.GetRawText(),
            out decimal _));
    }
}
