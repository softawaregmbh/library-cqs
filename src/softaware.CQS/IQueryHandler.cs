using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace softaware.CQS
{
    public interface IQueryHandler<TQuery, TResult>
        where TQuery : IQuery<TResult>
    {
        Task<TResult> HandleAsync(TQuery query);
    }
}
