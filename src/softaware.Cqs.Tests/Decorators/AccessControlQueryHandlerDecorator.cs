﻿using System.Threading.Tasks;
using softaware.Cqs.Tests.Fakes;

namespace softaware.Cqs.Tests.Decorators
{
    public class AccessControlQueryHandlerDecorator<TQuery, TResult> : IQueryHandler<TQuery, TResult>
        where TQuery : IQuery<TResult>, IAccessChecked
    {
        private readonly IQueryHandler<TQuery, TResult> decoratee;

        public AccessControlQueryHandlerDecorator(IQueryHandler<TQuery, TResult> decoratee)
        {
            this.decoratee = decoratee;
        }

        public async Task<TResult> HandleAsync(TQuery query)
        {
            query.AccessCheckHasBeenEvaluated = true;
            return await decoratee.HandleAsync(query);
        }
    }
}
