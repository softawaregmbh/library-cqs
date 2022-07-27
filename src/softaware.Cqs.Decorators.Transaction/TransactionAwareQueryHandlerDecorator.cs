using System.Transactions;

namespace softaware.Cqs.Decorators.Transaction;

/// <summary>
/// A decorator for creating a <see cref="TransactionScope"/> for the query handler.
/// The transaction gets committed when the decorated handler successfully executes.
/// </summary>
/// <typeparam name="TQuery">The type of the request to execute.</typeparam>
/// <typeparam name="TResult">The type of the result.</typeparam>
public class TransactionAwareQueryHandlerDecorator<TQuery, TResult> : TransactionAwareRequestHandlerDecorator<TQuery, TResult>
    where TQuery : IQuery<TResult>
{
    public TransactionAwareQueryHandlerDecorator(IRequestHandler<TQuery, TResult> decoratee) : base(decoratee)
    {
    }
}
