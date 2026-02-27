namespace CleanArchitecture.Application.Services;

using System.Threading.Tasks;
using CleanArchitecture.Domain.ValueObjects;

/// <summary>
/// </summary>
public interface ICurrencyExchange
{
    Task<Money> Convert(Money originalAmount, Currency destinationCurrency);
}
