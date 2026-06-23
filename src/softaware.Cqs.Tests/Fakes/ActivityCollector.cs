using System.Collections.Concurrent;
using System.Diagnostics;

namespace softaware.Cqs.Tests.Fakes;

/// <summary>
/// Subscribes an <see cref="ActivityListener"/> to a given <see cref="ActivitySource"/> name
/// and collects all activities that are stopped while it is active.
/// </summary>
public sealed class ActivityCollector : IDisposable
{
    private readonly ActivityListener listener;

    public ConcurrentBag<Activity> StoppedActivities { get; } = [];

    public ActivityCollector(string sourceName)
    {
        this.listener = new ActivityListener
        {
            ShouldListenTo = source => source.Name == sourceName,
            Sample = (ref ActivityCreationOptions<ActivityContext> _) => ActivitySamplingResult.AllDataAndRecorded,
            ActivityStopped = activity => this.StoppedActivities.Add(activity)
        };

        ActivitySource.AddActivityListener(this.listener);
    }

    public void Dispose() => this.listener.Dispose();
}
