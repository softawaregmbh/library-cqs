using System.Transactions;

namespace softaware.Cqs.Decorators.Transaction;

/// <summary>
/// A decorator for creating a <see cref="TransactionScope"/> for the command handler.
/// The transaction gets committed when the decorated handler successfully executes.
/// </summary>
/// <typeparam name="TRequest">The type of the request to execute.</typeparam>
/// <typeparam name="TResult">The type of the result.</typeparam>
public class TransactionAwareCommandHandlerDecorator<TRequest, TResult> : TransactionAwareRequestHandlerDecorator<TRequest, TResult>
    where TRequest : ICommand<TResult>
{
    public TransactionAwareCommandHandlerDecorator(IRequestHandler<TRequest, TResult> decoratee) : base(decoratee)
    {
    }
}
