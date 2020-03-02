using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using softaware.Cqs.Tests.CQ.Contract.Queries;

namespace softaware.Cqs.Tests.CQ.Handlers.QueryHandlers
{
    internal class StartAndEndDateHandler : IQueryHandler<StartAndEndDate, bool>
    {
        public Task<bool> HandleAsync(StartAndEndDate query)
        {
            return Task.FromResult(true);
        }
    }
}
