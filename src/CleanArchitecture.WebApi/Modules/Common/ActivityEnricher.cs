namespace CleanArchitecture.WebApi.Modules.Common;

using System.Diagnostics;
using Serilog.Core;
using Serilog.Events;

/// <summary>
///     Enriches Serilog log events with OpenTelemetry trace and span IDs
///     from the current <see cref="Activity" />, enabling correlation
///     between logs and distributed traces in the Aspire dashboard.
/// </summary>
public sealed class ActivityEnricher : ILogEventEnricher
{
    /// <inheritdoc />
    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        var activity = Activity.Current;
        if (activity is null)
        {
            return;
        }

        logEvent.AddPropertyIfAbsent(
            propertyFactory.CreateProperty("TraceId", activity.TraceId.ToHexString()));
        logEvent.AddPropertyIfAbsent(
            propertyFactory.CreateProperty("SpanId", activity.SpanId.ToHexString()));
        logEvent.AddPropertyIfAbsent(
            propertyFactory.CreateProperty("ParentSpanId", activity.ParentSpanId.ToHexString()));
    }
}
