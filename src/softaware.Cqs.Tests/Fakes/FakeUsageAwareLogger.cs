using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using softaware.UsageAware;

namespace softaware.Cqs.Tests.Fakes
{
    public class FakeUsageAwareLogger : IUsageAwareLogger
    {
        public List<(string area, string action, IEnumerable<KeyValuePair<string, string>> additionalProperties)> TrackedEvents = new List<(string area, string action, IEnumerable<KeyValuePair<string, string>> additionalProperties)>();

        public Task TrackActionAsync(string area, string action, IEnumerable<KeyValuePair<string, string>> additionalProperties = null)
        {
            this.TrackedEvents.Add((area, action, additionalProperties));
            return Task.CompletedTask;
        }
    }
}
