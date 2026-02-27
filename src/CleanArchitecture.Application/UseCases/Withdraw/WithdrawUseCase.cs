// <copyright file="WithdrawUseCase.cs" company="Ivan Paulovich">
// Copyright © Ivan Paulovich. All rights reserved.
// </copyright>

namespace CleanArchitecture.Application.UseCases.Withdraw;

using System;
using System.Threading.Tasks;
using CleanArchitecture.Domain;
using CleanArchitecture.Domain.Debits;
using CleanArchitecture.Domain.ValueObjects;
using Microsoft.Extensions.Logging;
using Services;

/// <inheritdoc />
public sealed class WithdrawUseCase : IWithdrawUseCase
{
    private readonly IAccountFactory _accountFactory;
    private readonly IAccountRepository _accountRepository;
    private readonly ICurrencyExchange _currencyExchange;
    private readonly ILogger<WithdrawUseCase> _logger;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUserService _userService;
    private IOutputPort _outputPort;

    /// <summary>
    ///     Initializes a new instance of the <see cref="WithdrawUseCase" /> class.
    /// </summary>
    /// <param name="accountRepository">Account Repository.</param>
    /// <param name="unitOfWork">Unit Of Work.</param>
    /// <param name="accountFactory"></param>
    /// <param name="userService"></param>
    /// <param name="currencyExchange"></param>
    /// <param name="logger"></param>
    public WithdrawUseCase(
        IAccountRepository accountRepository,
        IUnitOfWork unitOfWork,
        IAccountFactory accountFactory,
        IUserService userService,
        ICurrencyExchange currencyExchange,
        ILogger<WithdrawUseCase> logger)
    {
        this._accountRepository = accountRepository;
        this._unitOfWork = unitOfWork;
        this._accountFactory = accountFactory;
        this._userService = userService;
        this._currencyExchange = currencyExchange;
        this._logger = logger;
        this._outputPort = new WithdrawPresenter();
    }

    /// <inheritdoc />
    public void SetOutputPort(IOutputPort outputPort) => this._outputPort = outputPort;

    /// <inheritdoc />
    public Task Execute(Guid accountId, decimal amount, string currency) =>
        this.Withdraw(
            new AccountId(accountId),
            new Money(amount, new Currency(currency)));

    private async Task Withdraw(AccountId accountId, Money withdrawAmount)
    {
        string externalUserId = this._userService
            .GetCurrentUserId();

        this._logger.LogInformation(
            "Processing withdrawal of {Amount} {Currency} from account {AccountId} for user {UserId}",
            withdrawAmount.Amount, withdrawAmount.Currency, accountId, externalUserId);

        IAccount account = await this._accountRepository
            .Find(accountId, externalUserId)
            .ConfigureAwait(false);

        if (account is Account withdrawAccount)
        {
            Money localCurrencyAmount =
                await this._currencyExchange
                    .Convert(withdrawAmount, withdrawAccount.Currency)
                    .ConfigureAwait(false);

            Debit debit = this._accountFactory
                .NewDebit(withdrawAccount, localCurrencyAmount, DateTime.UtcNow);

            if (withdrawAccount.GetCurrentBalance().Subtract(debit.Amount).Amount < 0)
            {
                this._logger.LogWarning(
                    "Withdrawal denied: insufficient funds in account {AccountId}",
                    accountId);
                this._outputPort?.OutOfFunds();
                return;
            }

            await this.Withdraw(withdrawAccount, debit)
                .ConfigureAwait(false);

            this._logger.LogInformation(
                "Withdrawal of {Amount} {Currency} completed for account {AccountId}",
                localCurrencyAmount.Amount, localCurrencyAmount.Currency, accountId);

            this._outputPort.Ok(debit, withdrawAccount);
            return;
        }

        this._logger.LogWarning("Withdrawal failed: account {AccountId} not found", accountId);
        this._outputPort.NotFound();
    }

    private async Task Withdraw(Account account, Debit debit)
    {
        account.Withdraw(debit);

        await this._accountRepository
            .Update(account, debit)
            .ConfigureAwait(false);

        await this._unitOfWork
            .Save()
            .ConfigureAwait(false);
    }
}
