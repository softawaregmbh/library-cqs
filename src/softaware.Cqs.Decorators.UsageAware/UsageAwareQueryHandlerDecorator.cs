using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace softaware.Cqs.Decorators.UsageAware
{
    public class UsageAwareQueryHandlerDecorator<TQuery, TResult> : IQueryHandler<TQuery, TResult>
        where TQuery : IQuery<TResult>
    {
        private readonly UsageAwareQueryLogger<TQuery, TResult> logger;
        private readonly IQueryHandler<TQuery, TResult> decoratee;

        public UsageAwareQueryHandlerDecorator(
            UsageAwareQueryLogger<TQuery, TResult> logger,
            IQueryHandler<TQuery, TResult> decoratee)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.decoratee = decoratee ?? throw new ArgumentNullException(nameof(logger));
        }

        public Task<TResult> HandleAsync(TQuery query)
        {
            return this.logger.TimeAndLogQueryAsync(() => this.decoratee.HandleAsync(query));
        }
    }

}
