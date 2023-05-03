using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Extensibility;

namespace softaware.Cqs.Tests.Fakes;
public static class FakeTelemetryClient
{
    public static (TelemetryClient TelemetryClient, MockTelemetryChannel TelemetryChannel) Create()
    {
        var mockTelemetryChannel = new MockTelemetryChannel();
        var mockTelemetryConfig = new TelemetryConfiguration
        {
            TelemetryChannel = mockTelemetryChannel,
            ConnectionString = $"InstrumentationKey={Guid.NewGuid()}"
        };

        var mockTelemetryClient = new TelemetryClient(mockTelemetryConfig);
        return (mockTelemetryClient, mockTelemetryChannel);
    }
}
