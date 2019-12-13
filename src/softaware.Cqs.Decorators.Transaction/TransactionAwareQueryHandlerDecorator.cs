#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
using System.Threading.Tasks;
using System.Transactions;

namespace softaware.Cqs.Decorators.Transaction
{
    /// <summary>
    /// A decorator for creating a <see cref="TransactionScope"/> for the query handler.
    /// The transaction gets committed when the decorated handler successfully executes.
    /// </summary>
    /// <typeparam name="TQuery">The type of the query to execute.</typeparam>
    /// <typeparam name="TResult">The type of the query result.</typeparam>
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
