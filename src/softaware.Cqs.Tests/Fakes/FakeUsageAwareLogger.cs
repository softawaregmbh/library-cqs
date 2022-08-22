using softaware.UsageAware;

namespace softaware.Cqs.Tests.Fakes;

public class FakeUsageAwareLogger : IUsageAwareLogger
{
    public List<(string area, string action, IEnumerable<KeyValuePair<string, string>>? additionalProperties)> TrackedEvents { get; } =
        new List<(string area, string action, IEnumerable<KeyValuePair<string, string>>? additionalProperties)>();

    public Task TrackActionAsync(string area, string action, IEnumerable<KeyValuePair<string, string>>? additionalProperties = null)
    {
        this.TrackedEvents.Add((area, action, additionalProperties));
        return Task.CompletedTask;
    }
}
