// <copyright file="TransferUseCase.cs" company="Ivan Paulovich">
// Copyright © Ivan Paulovich. All rights reserved.
// </copyright>

namespace CleanArchitecture.Application.UseCases.Transfer;

using System;
using System.Threading.Tasks;
using CleanArchitecture.Domain;
using CleanArchitecture.Domain.Credits;
using CleanArchitecture.Domain.Debits;
using CleanArchitecture.Domain.ValueObjects;
using Microsoft.Extensions.Logging;
using Services;

/// <inheritdoc />
public sealed class TransferUseCase : ITransferUseCase
{
    private readonly IAccountFactory _accountFactory;
    private readonly IAccountRepository _accountRepository;
    private readonly ICurrencyExchange _currencyExchange;
    private readonly ILogger<TransferUseCase> _logger;
    private readonly IUnitOfWork _unitOfWork;
    private IOutputPort? _outputPort;

    /// <summary>
    ///     Initializes a new instance of the <see cref="TransferUseCase" /> class.
    /// </summary>
    /// <param name="accountRepository">Account Repository.</param>
    /// <param name="unitOfWork">Unit Of Work.</param>
    /// <param name="accountFactory"></param>
    /// <param name="currencyExchange"></param>
    /// <param name="logger"></param>
    public TransferUseCase(
        IAccountRepository accountRepository,
        IUnitOfWork unitOfWork,
        IAccountFactory accountFactory,
        ICurrencyExchange currencyExchange,
        ILogger<TransferUseCase> logger)
    {
        this._accountRepository = accountRepository;
        this._unitOfWork = unitOfWork;
        this._accountFactory = accountFactory;
        this._currencyExchange = currencyExchange;
        this._logger = logger;
        this._outputPort = new TransferPresenter();
    }

    /// <inheritdoc />
    public void SetOutputPort(IOutputPort outputPort) => this._outputPort = outputPort;

    /// <inheritdoc />
    public Task Execute(Guid originAccountId, Guid destinationAccountId, decimal amount, string currency) =>
        this.Transfer(
            new AccountId(originAccountId),
            new AccountId(destinationAccountId),
            new Money(amount, new Currency(currency)));

    private async Task Transfer(AccountId originAccountId, AccountId destinationAccountId,
        Money transferAmount)
    {
        this._logger.LogInformation(
            "Processing transfer of {Amount} {Currency} from account {OriginAccountId} to account {DestinationAccountId}",
            transferAmount.Amount, transferAmount.Currency, originAccountId, destinationAccountId);

        IAccount originAccount = await this._accountRepository
            .GetAccount(originAccountId)
            .ConfigureAwait(false);

        IAccount destinationAccount = await this._accountRepository
            .GetAccount(destinationAccountId)
            .ConfigureAwait(false);

        if (originAccount is Account withdrawAccount && destinationAccount is Account depositAccount)
        {
            Money localCurrencyAmount =
                await this._currencyExchange
                    .Convert(transferAmount, withdrawAccount.Currency)
                    .ConfigureAwait(false);

            Debit debit = this._accountFactory
                .NewDebit(withdrawAccount, localCurrencyAmount, DateTime.UtcNow);

            if (withdrawAccount.GetCurrentBalance().Subtract(debit.Amount).Amount < 0)
            {
                this._logger.LogWarning(
                    "Transfer denied: insufficient funds in origin account {OriginAccountId}",
                    originAccountId);
                this._outputPort?.OutOfFunds();
                return;
            }

            await this.Withdraw(withdrawAccount, debit)
                .ConfigureAwait(false);

            Money destinationCurrencyAmount =
                await this._currencyExchange
                    .Convert(transferAmount, depositAccount.Currency)
                    .ConfigureAwait(false);

            Credit credit = this._accountFactory
                .NewCredit(depositAccount, destinationCurrencyAmount, DateTime.UtcNow);

            await this.Deposit(depositAccount, credit)
                .ConfigureAwait(false);

            this._logger.LogInformation(
                "Transfer of {Amount} {Currency} completed from account {OriginAccountId} to account {DestinationAccountId}",
                transferAmount.Amount, transferAmount.Currency, originAccountId, destinationAccountId);

            this._outputPort?.Ok(withdrawAccount, debit, depositAccount, credit);
            return;
        }

        this._logger.LogWarning(
            "Transfer failed: origin account {OriginAccountId} or destination account {DestinationAccountId} not found",
            originAccountId, destinationAccountId);
        this._outputPort?.NotFound();
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
