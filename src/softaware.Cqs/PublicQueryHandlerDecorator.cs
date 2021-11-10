using System;
using System.Threading;
using System.Threading.Tasks;

namespace softaware.Cqs
{
    /// <summary>
    /// This decorator just delegates to the next decorator or to the actual handler.
    /// </summary>
    /// <remarks>
    /// This is needed since the introduction of the default interface implementation for the <see cref="IQueryHandler{TQuery, TResult}.HandleAsync(TQuery, CancellationToken)"/> overload,
    /// so that this overload can corretly be resolved by the DI container. See https://jeremybytes.blogspot.com/2019/09/c-8-interfaces-dynamic-and-default.html for details.
    /// </remarks>
    public class PublicQueryHandlerDecorator<TQuery, TResult>
        : IQueryHandler<TQuery, TResult> where TQuery : IQuery<TResult>
    {
        private readonly IQueryHandler<TQuery, TResult> decoratee;

        /// <summary>
        /// Initializes a new instance of the <see cref="PublicQueryHandlerDecorator{TQuery, TResult}"/> class.
        /// </summary>
        /// <param name="decoratee">The command handler to decorate.</param>
        public PublicQueryHandlerDecorator(IQueryHandler<TQuery, TResult> decoratee)
        {
            this.decoratee = decoratee ?? throw new ArgumentNullException(nameof(decoratee));
        }

        /// <inheritdoc/>
        public Task<TResult> HandleAsync(TQuery query)
        {
            return this.HandleAsync(query, default);
        }

        /// <inheritdoc/>
        public Task<TResult> HandleAsync(TQuery query, CancellationToken cancellationToken)
        {
            return this.decoratee.HandleAsync(query, cancellationToken);
        }
    }
}
