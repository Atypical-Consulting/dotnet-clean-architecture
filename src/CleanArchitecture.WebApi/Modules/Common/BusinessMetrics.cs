namespace CleanArchitecture.WebApi.Modules.Common;

using System.Collections.Generic;
using System.Diagnostics.Metrics;

/// <summary>
///     Custom business metrics for the Accounts API.
///     Tracks use case executions (deposits, withdrawals, transfers, etc.)
///     via OpenTelemetry-compatible <see cref="System.Diagnostics.Metrics" />.
/// </summary>
public sealed class BusinessMetrics
{
    /// <summary>
    ///     The meter name used for OpenTelemetry registration.
    /// </summary>
    public const string MeterName = "AccountsApi.Business";

    private readonly Counter<long> _depositsTotal;
    private readonly Counter<long> _withdrawalsTotal;
    private readonly Counter<long> _transfersTotal;
    private readonly Counter<long> _accountsOpenedTotal;
    private readonly Counter<long> _accountsClosedTotal;
    private readonly Histogram<double> _transactionAmounts;

    /// <summary>
    ///     Initializes a new instance of the <see cref="BusinessMetrics" /> class.
    /// </summary>
    public BusinessMetrics(IMeterFactory meterFactory)
    {
        var meter = meterFactory.Create(MeterName);

        _depositsTotal = meter.CreateCounter<long>(
            "accountsapi.deposits.total",
            description: "Total number of deposit transactions");

        _withdrawalsTotal = meter.CreateCounter<long>(
            "accountsapi.withdrawals.total",
            description: "Total number of withdrawal transactions");

        _transfersTotal = meter.CreateCounter<long>(
            "accountsapi.transfers.total",
            description: "Total number of transfer transactions");

        _accountsOpenedTotal = meter.CreateCounter<long>(
            "accountsapi.accounts_opened.total",
            description: "Total number of accounts opened");

        _accountsClosedTotal = meter.CreateCounter<long>(
            "accountsapi.accounts_closed.total",
            description: "Total number of accounts closed");

        _transactionAmounts = meter.CreateHistogram<double>(
            "accountsapi.transaction.amount",
            unit: "currency",
            description: "Distribution of transaction amounts");
    }

    /// <summary>Records a deposit transaction.</summary>
    public void RecordDeposit(decimal amount, string currency)
    {
        _depositsTotal.Add(1, new KeyValuePair<string, object?>("currency", currency));
        _transactionAmounts.Record((double)amount,
            new KeyValuePair<string, object?>("type", "deposit"),
            new KeyValuePair<string, object?>("currency", currency));
    }

    /// <summary>Records a withdrawal transaction.</summary>
    public void RecordWithdrawal(decimal amount, string currency)
    {
        _withdrawalsTotal.Add(1, new KeyValuePair<string, object?>("currency", currency));
        _transactionAmounts.Record((double)amount,
            new KeyValuePair<string, object?>("type", "withdrawal"),
            new KeyValuePair<string, object?>("currency", currency));
    }

    /// <summary>Records a transfer transaction.</summary>
    public void RecordTransfer(decimal amount, string currency)
    {
        _transfersTotal.Add(1, new KeyValuePair<string, object?>("currency", currency));
        _transactionAmounts.Record((double)amount,
            new KeyValuePair<string, object?>("type", "transfer"),
            new KeyValuePair<string, object?>("currency", currency));
    }

    /// <summary>Records an account opened event.</summary>
    public void RecordAccountOpened() => _accountsOpenedTotal.Add(1);

    /// <summary>Records an account closed event.</summary>
    public void RecordAccountClosed() => _accountsClosedTotal.Add(1);
}
