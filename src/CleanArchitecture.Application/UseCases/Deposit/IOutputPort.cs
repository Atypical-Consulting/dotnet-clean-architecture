// <copyright file="IOutputPort.cs" company="Ivan Paulovich">
// Copyright © Ivan Paulovich. All rights reserved.
// </copyright>

namespace CleanArchitecture.Application.UseCases.Deposit;

using CleanArchitecture.Domain;
using CleanArchitecture.Domain.Credits;

/// <summary>
///     Output Port.
/// </summary>
public interface IOutputPort
{
    /// <summary>
    ///     Invalid input.
    /// </summary>
    void Invalid();

    /// <summary>
    ///     Deposited.
    /// </summary>
    void Ok(Credit credit, Account account);

    /// <summary>
    ///     Not found.
    /// </summary>
    void NotFound();
}
