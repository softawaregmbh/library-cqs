using System.Threading.Tasks;
using softaware.Cqs.Tests.CQ.Contract.Queries;

namespace softaware.Cqs.Tests.CQ.Handlers.QueryHandlers
{
    public class AccessCheckedQueryHandler : IQueryHandler<AccessCheckedQuery, bool>
    {
        public Task<bool> HandleAsync(AccessCheckedQuery query)
        {
            return Task.FromResult(true);
        }
    }
}
