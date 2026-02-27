namespace CleanArchitecture.WebApi.Modules;

using CleanArchitecture.Application.Services;
using CleanArchitecture.Application.UseCases.CloseAccount;
using CleanArchitecture.Application.UseCases.Deposit;
using CleanArchitecture.Application.UseCases.GetAccount;
using CleanArchitecture.Application.UseCases.GetAccounts;
using CleanArchitecture.Application.UseCases.OpenAccount;
using CleanArchitecture.Application.UseCases.Transfer;
using CleanArchitecture.Application.UseCases.Withdraw;
using Microsoft.Extensions.DependencyInjection;

/// <summary>
///     Adds Use Cases classes.
/// </summary>
public static class UseCasesExtensions
{
    /// <summary>
    ///     Adds Use Cases to the ServiceCollection.
    /// </summary>
    /// <param name="services">Service Collection.</param>
    /// <returns>The modified instance.</returns>
    public static IServiceCollection AddUseCases(this IServiceCollection services)
    {
        services.AddScoped<Notification, Notification>();

        services.AddScoped<ICloseAccountUseCase, CloseAccountUseCase>();
        services.Decorate<ICloseAccountUseCase, CloseAccountValidationUseCase>();

        services.AddScoped<IDepositUseCase, DepositUseCase>();
        services.Decorate<IDepositUseCase, DepositValidationUseCase>();

        services.AddScoped<IGetAccountUseCase, GetAccountUseCase>();
        services.Decorate<IGetAccountUseCase, GetAccountValidationUseCase>();

        services.AddScoped<IGetAccountsUseCase, GetAccountsUseCase>();

        services.AddScoped<IOpenAccountUseCase, OpenAccountUseCase>();
        services.Decorate<IOpenAccountUseCase, OpenAccountValidationUseCase>();

        services.AddScoped<ITransferUseCase, TransferUseCase>();
        services.Decorate<ITransferUseCase, TransferValidationUseCase>();

        services.AddScoped<IWithdrawUseCase, WithdrawUseCase>();
        services.Decorate<IWithdrawUseCase, WithdrawValidationUseCase>();

        return services;
    }
}
