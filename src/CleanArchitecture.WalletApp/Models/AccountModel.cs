namespace CleanArchitecture.WalletApp.Models;

public sealed class AccountModel
{
    public Guid AccountId { get; set; }
    public decimal CurrentBalance { get; set; }
    public string Currency { get; set; } = string.Empty;
}
