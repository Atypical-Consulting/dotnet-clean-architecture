// <copyright file="CloseAccountUseCase.cs" company="Ivan Paulovich">
// Copyright © Ivan Paulovich. All rights reserved.
// </copyright>

namespace Application.UseCases.CloseAccount;

using System;
using System.Threading.Tasks;
using Domain;
using Domain.ValueObjects;
using Microsoft.Extensions.Logging;
using Services;

/// <inheritdoc />
public sealed class CloseAccountUseCase : ICloseAccountUseCase
{
    private readonly IAccountRepository _accountRepository;
    private readonly ILogger<CloseAccountUseCase> _logger;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUserService _userService;

    private IOutputPort _outputPort;

    /// <summary>
    ///     Initializes a new instance of the <see cref="CloseAccountUseCase" /> class.
    /// </summary>
    /// <param name="accountRepository">Account Repository.</param>
    /// <param name="userService">User Service.</param>
    /// <param name="unitOfWork"></param>
    /// <param name="logger"></param>
    public CloseAccountUseCase(
        IAccountRepository accountRepository,
        IUserService userService,
        IUnitOfWork unitOfWork,
        ILogger<CloseAccountUseCase> logger)
    {
        this._accountRepository = accountRepository;
        this._userService = userService;
        this._unitOfWork = unitOfWork;
        this._logger = logger;
        this._outputPort = new CloseAccountPresenter();
    }

    /// <inheritdoc />
    public void SetOutputPort(IOutputPort outputPort) => this._outputPort = outputPort;

    /// <inheritdoc />
    public Task Execute(Guid accountId)
    {
        string externalUserId = this._userService
            .GetCurrentUserId();

        return this.CloseAccountInternal(new AccountId(accountId), externalUserId);
    }

    private async Task CloseAccountInternal(AccountId accountId, string externalUserId)
    {
        this._logger.LogInformation(
            "Closing account {AccountId} for user {UserId}",
            accountId, externalUserId);

        IAccount account = await this._accountRepository
            .Find(accountId, externalUserId)
            .ConfigureAwait(false);

        if (account is Account closingAccount)
        {
            if (!closingAccount.IsClosingAllowed())
            {
                this._logger.LogWarning(
                    "Account closure denied: account {AccountId} still has funds",
                    accountId);
                this._outputPort.HasFunds();
                return;
            }

            await this.Close(closingAccount)
                .ConfigureAwait(false);

            this._logger.LogInformation(
                "Account {AccountId} closed successfully",
                accountId);

            this._outputPort.Ok(closingAccount);
            return;
        }

        this._logger.LogWarning("Account closure failed: account {AccountId} not found", accountId);
        this._outputPort.NotFound();
    }

    private async Task Close(Account closeAccount)
    {
        await this._accountRepository
            .Delete(closeAccount.AccountId)
            .ConfigureAwait(false);

        await this._unitOfWork
            .Save()
            .ConfigureAwait(false);
    }
}
