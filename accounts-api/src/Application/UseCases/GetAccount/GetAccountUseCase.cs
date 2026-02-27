// <copyright file="GetAccountUseCase.cs" company="Ivan Paulovich">
// Copyright © Ivan Paulovich. All rights reserved.
// </copyright>

namespace Application.UseCases.GetAccount;

using System;
using System.Threading.Tasks;
using Domain;
using Domain.ValueObjects;
using Microsoft.Extensions.Logging;

/// <inheritdoc />
public sealed class GetAccountUseCase : IGetAccountUseCase
{
    private readonly IAccountRepository _accountRepository;
    private readonly ILogger<GetAccountUseCase> _logger;
    private IOutputPort _outputPort;

    /// <summary>
    ///     Initializes a new instance of the <see cref="GetAccountUseCase" /> class.
    /// </summary>
    /// <param name="accountRepository">Account Repository.</param>
    /// <param name="logger"></param>
    public GetAccountUseCase(
        IAccountRepository accountRepository,
        ILogger<GetAccountUseCase> logger)
    {
        this._accountRepository = accountRepository;
        this._logger = logger;
        this._outputPort = new GetAccountPresenter();
    }

    /// <inheritdoc />
    public void SetOutputPort(IOutputPort outputPort) => this._outputPort = outputPort;

    /// <inheritdoc />
    public Task Execute(Guid accountId) =>
        this.GetAccountInternal(new AccountId(accountId));

    private async Task GetAccountInternal(AccountId accountId)
    {
        this._logger.LogDebug("Retrieving account {AccountId}", accountId);

        IAccount account = await this._accountRepository
            .GetAccount(accountId)
            .ConfigureAwait(false);

        if (account is Account getAccount)
        {
            this._outputPort.Ok(getAccount);
            return;
        }

        this._logger.LogWarning("Account {AccountId} not found", accountId);
        this._outputPort.NotFound();
    }
}
