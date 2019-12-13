using System.Threading;
using System.Threading.Tasks;

namespace softaware.Cqs
{
    /// <summary>
    /// The query handler which handles the specified query.
    /// </summary>
    /// <typeparam name="TQuery">The type of the query to handle.</typeparam>
    /// <typeparam name="TResult">The type of the query result.</typeparam>
    public interface IQueryHandler<TQuery, TResult>
        where TQuery : IQuery<TResult>
    {
        /// <summary>
        /// Handles a query.
        /// </summary>
        /// <param name="query">The query to handle.</param>
        /// <returns></returns>
        Task<TResult> HandleAsync(TQuery query);

        /// <summary>
        /// Handles a query which cancellation support.
        /// </summary>
        /// <param name="query">The query to handle.</param>
        /// <param name="cancellationToken">The cancellation token when requesting the cancellation of the execution.</param>
        /// <returns>The query result.</returns>
        Task<TResult> HandleAsync(TQuery query, CancellationToken cancellationToken = default)
        {
            return this.HandleAsync(query);
        }
    }
}
