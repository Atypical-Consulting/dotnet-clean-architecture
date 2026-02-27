// <copyright file="DepositUseCase.cs" company="Ivan Paulovich">
// Copyright © Ivan Paulovich. All rights reserved.
// </copyright>

namespace Application.UseCases.Deposit;

using System;
using System.Threading.Tasks;
using Domain;
using Domain.Credits;
using Domain.ValueObjects;
using Microsoft.Extensions.Logging;
using Services;

/// <inheritdoc />
public sealed class DepositUseCase : IDepositUseCase
{
    private readonly IAccountFactory _accountFactory;
    private readonly IAccountRepository _accountRepository;
    private readonly ICurrencyExchange _currencyExchange;
    private readonly ILogger<DepositUseCase> _logger;
    private readonly IUnitOfWork _unitOfWork;
    private IOutputPort _outputPort;

    /// <summary>
    ///     Initializes a new instance of the <see cref="DepositUseCase" /> class.
    /// </summary>
    /// <param name="accountRepository">Account Repository.</param>
    /// <param name="unitOfWork">Unit Of Work.</param>
    /// <param name="accountFactory"></param>
    /// <param name="currencyExchange"></param>
    /// <param name="logger"></param>
    public DepositUseCase(
        IAccountRepository accountRepository,
        IUnitOfWork unitOfWork,
        IAccountFactory accountFactory,
        ICurrencyExchange currencyExchange,
        ILogger<DepositUseCase> logger)
    {
        this._accountRepository = accountRepository;
        this._unitOfWork = unitOfWork;
        this._accountFactory = accountFactory;
        this._currencyExchange = currencyExchange;
        this._logger = logger;
        this._outputPort = new DepositPresenter();
    }

    /// <inheritdoc />
    public void SetOutputPort(IOutputPort outputPort) => this._outputPort = outputPort;

    /// <inheritdoc />
    public Task Execute(Guid accountId, decimal amount, string currency) =>
        this.Deposit(
            new AccountId(accountId),
            new Money(amount, new Currency(currency)));

    private async Task Deposit(AccountId accountId, Money amount)
    {
        this._logger.LogInformation(
            "Processing deposit of {Amount} {Currency} to account {AccountId}",
            amount.Amount, amount.Currency, accountId);

        IAccount account = await this._accountRepository
            .GetAccount(accountId)
            .ConfigureAwait(false);

        if (account is Account depositAccount)
        {
            Money convertedAmount =
                await this._currencyExchange
                    .Convert(amount, depositAccount.Currency)
                    .ConfigureAwait(false);

            Credit credit = this._accountFactory
                .NewCredit(depositAccount, convertedAmount, DateTime.Now);

            await this.Deposit(depositAccount, credit)
                .ConfigureAwait(false);

            this._logger.LogInformation(
                "Deposit of {Amount} {Currency} completed for account {AccountId}",
                convertedAmount.Amount, convertedAmount.Currency, accountId);

            this._outputPort.Ok(credit, depositAccount);
            return;
        }

        this._logger.LogWarning("Deposit failed: account {AccountId} not found", accountId);
        this._outputPort.NotFound();
    }

    private async Task Deposit(Account account, Credit credit)
    {
        account.Deposit(credit);

        await this._accountRepository
            .Update(account, credit)
            .ConfigureAwait(false);

        await this._unitOfWork
            .Save()
            .ConfigureAwait(false);
    }
}
