using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace softaware.Cqs.Decorators.UsageAware
{
    public class UsageAwareQueryLogger<TQuery, TResult> : UsageAwareLogger<TQuery>
        where TQuery : IQuery<TResult>
    {
        //// TODO
        ////public UsageAwareQueryLogger(IUsageAwareLogger logger)
        ////    : base(logger)
        ////{
        ////}

        public async Task<TResult> TimeAndLogQueryAsync(Func<Task<TResult>> query)
        {
            var task = await this.TimeAndLogAsync(query, LogType.Query);
            return ((Task<TResult>)task).Result;
        }
    }
}
