using System.Threading;
using System.Threading.Tasks;

namespace softaware.Cqs
{
    public interface IQueryProcessor
    {
        Task<TResult> ExecuteAsync<TResult>(IQuery<TResult> query, CancellationToken cancellationToken = default);
    }
}
