using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using softaware.Cqs.Tests.CQ.Contract.Queries;

namespace softaware.Cqs.Tests.CQ.Handlers.QueryHandlers
{
    public class GetSquareHandler : IQueryHandler<GetSquare, int>
    {
        public Task<int> HandleAsync(GetSquare query)
        {
            return Task.FromResult(query.Value * query.Value);
        }
    }
}
