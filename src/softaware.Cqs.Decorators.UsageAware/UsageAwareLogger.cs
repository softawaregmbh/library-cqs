#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using softaware.UsageAware;

namespace softaware.Cqs.Decorators.UsageAware
{
    /// <summary>
    /// Logs and measures duration of the execution of a command or a query.
    /// </summary>
    public class UsageAwareLogger
    {
        protected struct ActionInfo
        {
            public ActionInfo(string area, string action)
            {
                this.Area = area;
                this.Action = action;
            }

            public string Area { get; }
            public string Action { get; }
        }

        protected enum LogType
        {
            Command,
            Query
        }

        private readonly IUsageAwareLogger logger;
        protected readonly ConcurrentDictionary<Type, ActionInfo> actionInfos = new ConcurrentDictionary<Type, ActionInfo>();

        public UsageAwareLogger(IUsageAwareLogger logger)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        protected async Task<Task> TimeAndLogAsync<T>(Func<Task> commandOrQuery, LogType type)
        {
            var watch = Stopwatch.StartNew();

            var task = commandOrQuery();
            await task;

            var additionalProperties = new Dictionary<string, string>()
            {
                { "duration", watch.Elapsed.ToString() },
                { "type", type.ToString() }
            };

            var actionInfo = this.actionInfos.GetOrAdd(
                typeof(T),
                type => new ActionInfo(
                    area: type.Namespace.Substring(type.Namespace.LastIndexOf('.') + 1),
                    action: type.Name));

            await this.logger.TrackActionAsync(actionInfo.Area, actionInfo.Action, additionalProperties);

            return task;
        }
    }
}
