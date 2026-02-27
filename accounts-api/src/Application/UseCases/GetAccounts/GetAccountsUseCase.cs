// <copyright file="GetAccountsUseCase.cs" company="Ivan Paulovich">
// Copyright © Ivan Paulovich. All rights reserved.
// </copyright>

namespace Application.UseCases.GetAccounts;

using System.Collections.Generic;
using System.Threading.Tasks;
using Domain;
using Microsoft.Extensions.Logging;
using Services;

/// <inheritdoc />
public sealed class GetAccountsUseCase : IGetAccountsUseCase
{
    private readonly IAccountRepository _accountRepository;
    private readonly ILogger<GetAccountsUseCase> _logger;
    private readonly IUserService _userService;
    private IOutputPort _outputPort;

    /// <summary>
    ///     Initializes a new instance of the <see cref="GetAccountsUseCase" /> class.
    /// </summary>
    /// <param name="userService">User Service.</param>
    /// <param name="accountRepository">Customer Repository.</param>
    /// <param name="logger"></param>
    public GetAccountsUseCase(
        IUserService userService,
        IAccountRepository accountRepository,
        ILogger<GetAccountsUseCase> logger)
    {
        this._userService = userService;
        this._accountRepository = accountRepository;
        this._logger = logger;
        this._outputPort = new GetAccountPresenter();
    }

    /// <inheritdoc />
    public void SetOutputPort(IOutputPort outputPort) => this._outputPort = outputPort;

    /// <inheritdoc />
    public Task Execute()
    {
        string externalUserId = this._userService
            .GetCurrentUserId();

        return this.GetAccounts(externalUserId);
    }

    private async Task GetAccounts(string externalUserId)
    {
        this._logger.LogDebug("Retrieving accounts for user {UserId}", externalUserId);

        IList<Account> accounts = await this._accountRepository
            .GetAccounts(externalUserId)
            .ConfigureAwait(false) ?? [];

        this._logger.LogDebug(
            "Retrieved {AccountCount} accounts for user {UserId}",
            accounts.Count, externalUserId);

        this._outputPort.Ok(accounts);
    }
}
