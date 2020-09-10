#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
using softaware.UsageAware;
using System;
using System.Threading.Tasks;

namespace softaware.Cqs.Decorators.UsageAware
{
    /// <summary>
    /// A <see cref="UsageAwareLogger{T}"/> for logging queries.
    /// </summary>
    /// <typeparam name="TQuery">The type of the query to log.</typeparam>
    /// <typeparam name="TResult">The type of the query result.</typeparam>
    public class UsageAwareQueryLogger<TQuery, TResult> : UsageAwareLogger<TQuery>
        where TQuery : IQuery<TResult>
    {
        public UsageAwareQueryLogger(IUsageAwareLogger logger)
            : base(logger)
        {
        }

        public async Task<TResult> TimeAndLogQueryAsync(Func<Task<TResult>> query)
        {
            var task = await this.TimeAndLogAsync(query, LogType.Query);
            return ((Task<TResult>)task).Result;
        }
    }
}
