// <copyright file="GetAccountPresenter.cs" company="Ivan Paulovich">
// Copyright © Ivan Paulovich. All rights reserved.
// </copyright>

namespace CleanArchitecture.Application.UseCases.GetAccount;

using CleanArchitecture.Domain;

/// <summary>
///     Deposit Presenter.
/// </summary>
public sealed class GetAccountPresenter : IOutputPort
{
    public Account? Account { get; private set; }
    public bool? IsNotFound { get; private set; }
    public bool? InvalidOutput { get; private set; }
    public void Invalid() => this.InvalidOutput = true;
    public void NotFound() => this.IsNotFound = true;

    public void Ok(Account account) => this.Account = account;
}
