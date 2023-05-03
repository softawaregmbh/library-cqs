using System.Collections.Concurrent;
using Microsoft.ApplicationInsights.Channel;

namespace softaware.Cqs.Tests.Fakes;
public class MockTelemetryChannel : ITelemetryChannel
{
    public ConcurrentBag<ITelemetry> SentTelemetries { get; } = new ConcurrentBag<ITelemetry>();
    public bool IsFlushed { get; private set; }
    public bool? DeveloperMode { get; set; }
    public string EndpointAddress { get; set; } = string.Empty;

    public void Send(ITelemetry item) => this.SentTelemetries.Add(item);

    public void Flush() => this.IsFlushed = true;

    public void Dispose()
    {
    }
}
