using System.Threading;
using System.Threading.Tasks;

namespace softaware.Cqs
{
    /// <summary>
    /// Processes a specified query.
    /// </summary>
    public interface IQueryProcessor
    {
        /// <summary>
        /// Executes the specified query.
        /// </summary>
        /// <param name="query">The query to execute.</param>
        /// <param name="cancellationToken">The optional cancellation token for requesting cancellation of the query execution.</param>
        /// <returns>The query result.</returns>
        Task<TResult> ExecuteAsync<TResult>(IQuery<TResult> query, CancellationToken cancellationToken = default);
    }
}
