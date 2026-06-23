using System.Diagnostics;

namespace softaware.Cqs.Decorators.OpenTelemetry;

/// <summary>
/// Provides the <see cref="System.Diagnostics.ActivitySource"/> used by the CQS OpenTelemetry decorators.
/// </summary>
public static class CqsActivitySource
{
    /// <summary>
    /// The name of the <see cref="System.Diagnostics.ActivitySource"/>.
    /// Enable it by calling <c>AddSource(CqsActivitySource.Name)</c> on your <c>TracerProviderBuilder</c>.
    /// </summary>
    public const string Name = "softaware.Cqs";

    /// <summary>
    /// The <see cref="System.Diagnostics.ActivitySource"/> used to create activities for request handlers.
    /// </summary>
    public static readonly ActivitySource ActivitySource = new(Name);
}
