using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;

namespace softaware.CQS.Decorators.Transaction
{
    public class TransactionAwareQueryHandlerDecorator<TQuery, TResult> : IQueryHandler<TQuery, TResult>
        where TQuery : IQuery<TResult>
    {
        private readonly IQueryHandler<TQuery, TResult> decoratee;

        public TransactionAwareQueryHandlerDecorator(
            IQueryHandler<TQuery, TResult> decoratee)
        {
            this.decoratee = decoratee;
        }

        public async Task<TResult> HandleAsync(TQuery query)
        {
            TransactionOptions transactionOptions = new TransactionOptions
            {
                IsolationLevel = IsolationLevel.ReadCommitted
            };

            using (var tx = new TransactionScope(TransactionScopeOption.Required, transactionOptions, TransactionScopeAsyncFlowOption.Enabled))
            {
                var result = await this.decoratee.HandleAsync(query);
                tx.Complete();

                return result;
            }
        }
    }
}
