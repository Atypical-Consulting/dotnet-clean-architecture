namespace WalletApp.Models;

public sealed class GetAccountsResponse
{
    public List<AccountModel> Accounts { get; set; } = [];
}

public sealed class GetAccountResponse
{
    public AccountDetailsModel Account { get; set; } = new();
}

public sealed class OpenAccountResponse
{
    public AccountModel Account { get; set; } = new();
}

public sealed class DepositResponse
{
    public TransactionModel Transaction { get; set; } = new();
}

public sealed class WithdrawResponse
{
    public TransactionModel Transaction { get; set; } = new();
}

public sealed class TransferResponse
{
    public TransactionModel Transaction { get; set; } = new();
}

public sealed class CloseAccountResponse
{
    public Guid AccountId { get; set; }
}
