using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace softaware.Cqs
{
    public interface IQueryHandler<TQuery, TResult>
        where TQuery : IQuery<TResult>
    {
        Task<TResult> HandleAsync(TQuery query);
    }
}
