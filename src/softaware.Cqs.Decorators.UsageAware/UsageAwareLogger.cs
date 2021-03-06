﻿#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
using softaware.UsageAware;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace softaware.Cqs.Decorators.UsageAware
{
    /// <summary>
    /// Logs and measures duration of the execution of a command or a query.
    /// </summary>
    /// <typeparam name="T">The type of the command or the query to log.</typeparam>
    public class UsageAwareLogger<T>
    {
        protected enum LogType
        {
            Command,
            Query
        }

        private static readonly string Area;
        private static readonly string Action;

        private readonly IUsageAwareLogger logger;

        static UsageAwareLogger()
        {
            var type = typeof(T);
            Area = type.Namespace.Substring(type.Namespace.LastIndexOf('.') + 1);
            Action = type.Name;
        }

        public UsageAwareLogger(IUsageAwareLogger logger)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        protected async Task<Task> TimeAndLogAsync(Func<Task> commandOrQuery, LogType type)
        {
            var watch = Stopwatch.StartNew();

            var task = commandOrQuery();
            await task;

            var additionalProperties = new Dictionary<string, string>()
            {
                { "duration", watch.Elapsed.ToString() },
                { "type", type.ToString() }
            };

            await this.logger.TrackActionAsync(Area, Action, additionalProperties);

            return task;
        }
    }
}
