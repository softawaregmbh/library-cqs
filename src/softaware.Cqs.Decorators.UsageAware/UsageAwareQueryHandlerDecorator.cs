#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
using System;
using System.Threading;
using System.Threading.Tasks;

namespace softaware.Cqs.Decorators.UsageAware
{
    /// <summary>
    /// A decorator for tracking query executions with UsageAware.
    /// </summary>
    /// <typeparam name="TQuery">The type of the query to execute.</typeparam>
    /// <typeparam name="TResult">The type of the query result.</typeparam>
    public class UsageAwareQueryHandlerDecorator<TQuery, TResult> : IQueryHandler<TQuery, TResult>
        where TQuery : IQuery<TResult>
    {
        private readonly UsageAwareQueryLogger logger;
        private readonly IQueryHandler<TQuery, TResult> decoratee;

        public UsageAwareQueryHandlerDecorator(
            UsageAwareQueryLogger logger,
            IQueryHandler<TQuery, TResult> decoratee)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.decoratee = decoratee ?? throw new ArgumentNullException(nameof(logger));
        }

        public Task<TResult> HandleAsync(TQuery query) => this.HandleAsync(query, default);

        public Task<TResult> HandleAsync(TQuery query, CancellationToken cancellationToken)
        {
            return this.logger.TimeAndLogQueryAsync<TQuery, TResult>(() => this.decoratee.HandleAsync(query, cancellationToken));
        }
    }
}
