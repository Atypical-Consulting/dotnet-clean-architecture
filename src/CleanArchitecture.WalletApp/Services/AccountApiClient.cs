namespace CleanArchitecture.WalletApp.Services;

using System.Net.Http.Json;
using Models;

public sealed class AccountApiClient(HttpClient httpClient)
{
    public async Task<List<AccountModel>> GetAccountsAsync()
    {
        var response = await httpClient.GetFromJsonAsync<GetAccountsResponse>("api/v1/Accounts");
        return response?.Accounts ?? [];
    }

    public async Task<AccountDetailsModel?> GetAccountAsync(Guid accountId)
    {
        var response = await httpClient.GetFromJsonAsync<GetAccountResponse>($"api/v1/Accounts/{accountId}");
        return response?.Account;
    }

    public async Task<AccountModel?> OpenAccountAsync(decimal amount, string currency)
    {
        var content = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["amount"] = amount.ToString(),
            ["currency"] = currency
        });
        var response = await httpClient.PostAsync("api/v1/Accounts", content);
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<OpenAccountResponse>();
        return result?.Account;
    }

    public async Task<bool> DepositAsync(Guid accountId, decimal amount, string currency)
    {
        var content = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["amount"] = amount.ToString(),
            ["currency"] = currency
        });
        var response = await httpClient.PatchAsync($"api/v1/Transactions/{accountId}/Deposit", content);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> WithdrawAsync(Guid accountId, decimal amount, string currency)
    {
        var content = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["amount"] = amount.ToString(),
            ["currency"] = currency
        });
        var response = await httpClient.PatchAsync($"api/v1/Transactions/{accountId}/Withdraw", content);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> TransferAsync(Guid sourceAccountId, Guid destinationAccountId, decimal amount, string currency)
    {
        var content = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["amount"] = amount.ToString(),
            ["currency"] = currency
        });
        var response = await httpClient.PatchAsync($"api/v1/Transactions/{sourceAccountId}/{destinationAccountId}", content);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> CloseAccountAsync(Guid accountId)
    {
        var response = await httpClient.DeleteAsync($"api/v1/Accounts/{accountId}");
        return response.IsSuccessStatusCode;
    }
}
