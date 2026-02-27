// <copyright file="GetAccountPresenter.cs" company="Ivan Paulovich">
// Copyright © Ivan Paulovich. All rights reserved.
// </copyright>

namespace CleanArchitecture.Application.UseCases.Deposit;

using CleanArchitecture.Domain;
using CleanArchitecture.Domain.Credits;

/// <summary>
///     Deposit Presenter.
/// </summary>
public sealed class DepositPresenter : IOutputPort
{
    public Account? Account { get; private set; }
    public Credit? Credit { get; private set; }
    public bool? IsNotFound { get; private set; }
    public bool? InvalidOutput { get; private set; }
    public void Invalid() => this.InvalidOutput = true;
    public void NotFound() => this.IsNotFound = true;

    public void Ok(Credit credit, Account account)
    {
        this.Credit = credit;
        this.Account = account;
    }
}
