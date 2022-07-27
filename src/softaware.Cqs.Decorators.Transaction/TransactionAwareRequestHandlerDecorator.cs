using System.Transactions;

namespace softaware.Cqs.Decorators.Transaction;

/// <summary>
/// A decorator for creating a <see cref="TransactionScope"/> for the request handler.
/// The transaction gets committed when the decorated handler successfully executes.
/// </summary>
/// <typeparam name="TRequest">The type of the request to execute.</typeparam>
/// <typeparam name="TResult">The type of the result.</typeparam>
public class TransactionAwareRequestHandlerDecorator<TRequest, TResult> : IRequestHandler<TRequest, TResult>
    where TRequest : IRequest<TResult>
{
    private readonly IRequestHandler<TRequest, TResult> decoratee;

    public TransactionAwareRequestHandlerDecorator(IRequestHandler<TRequest, TResult> decoratee) =>
        this.decoratee = decoratee ?? throw new ArgumentNullException(nameof(decoratee));

    public Task<TResult> HandleAsync(TRequest query) => this.HandleAsync(query, default);

    public async Task<TResult> HandleAsync(TRequest query, CancellationToken cancellationToken)
    {
        TransactionOptions transactionOptions = new TransactionOptions
        {
            IsolationLevel = IsolationLevel.ReadCommitted
        };

        using (var tx = new TransactionScope(TransactionScopeOption.Required, transactionOptions, TransactionScopeAsyncFlowOption.Enabled))
        {
            var result = await this.decoratee.HandleAsync(query, cancellationToken);
            tx.Complete();

            return result;
        }
    }
}
