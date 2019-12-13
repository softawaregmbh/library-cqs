using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using softaware.Cqs.Tests.CQ.Contract.Commands;
using softaware.Cqs.Tests.CQ.Contract.Queries;

namespace softaware.Cqs.Tests.CQ.Handlers.QueryHandlers
{
    public class CallbackQueryHandler : IQueryHandler<CallbackQuery, int>
    {
        public Task<int> HandleAsync(CallbackQuery query)
        {
            query.Action();

            if (query.ShouldThrow)
            {
                throw new Exception("We throw here for testing the rollback of transactions.");
            }

            return Task.FromResult(1);
        }
    }
}
