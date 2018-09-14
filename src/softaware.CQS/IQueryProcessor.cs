using System.Threading.Tasks;

namespace softaware.CQS
{
    public interface IQueryProcessor
    {
        Task<TResult> ExecuteAsync<TResult>(IQuery<TResult> query);
    }
}
