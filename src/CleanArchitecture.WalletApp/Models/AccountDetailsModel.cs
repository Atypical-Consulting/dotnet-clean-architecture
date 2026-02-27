namespace CleanArchitecture.WalletApp.Models;

public sealed class AccountDetailsModel
{
    public Guid AccountId { get; set; }
    public decimal CurrentBalance { get; set; }
    public List<TransactionModel> Credits { get; set; } = [];
    public List<TransactionModel> Debits { get; set; } = [];
}
