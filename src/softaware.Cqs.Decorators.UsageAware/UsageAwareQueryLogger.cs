#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
using System;
using System.Threading.Tasks;
using softaware.UsageAware;

namespace softaware.Cqs.Decorators.UsageAware
{
    /// <summary>
    /// A <see cref="UsageAwareLogger"/> for logging queries.
    /// </summary>
    public class UsageAwareQueryLogger : UsageAwareLogger
    {
        public UsageAwareQueryLogger(IUsageAwareLogger logger)
            : base(logger)
        {
        }

        public async Task<TResult> TimeAndLogQueryAsync<TQuery, TResult>(Func<Task<TResult>> query)
            where TQuery : IQuery<TResult>
        {
            var task = await this.TimeAndLogAsync<TQuery>(query, LogType.Query);
            return ((Task<TResult>)task).Result;
        }
    }
}
