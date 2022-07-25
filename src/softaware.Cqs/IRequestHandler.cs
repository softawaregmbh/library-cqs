namespace softaware.Cqs;

/// <summary>
/// Base interface for <see cref="IQueryHandler{TQuery, TResult}"/> and <see cref="ICommandHandler{TCommand, TResult}"/>.
/// </summary>
/// <typeparam name="TRequest">The type of the query to handle.</typeparam>
/// <typeparam name="TResult">The type of the query result.</typeparam>
/// <remarks>
/// This interface only exists as a common base type for <see cref="IQueryHandler{TQuery, TResult}"/> and <see cref="ICommandHandler{TCommand, TResult}"/>.
/// It should not be implemented directly by handlers, but it can be useful for implementing decorators that decorate both types.
/// </remarks>
public interface IRequestHandler<TRequest, TResult>
    where TRequest : IRequest<TResult>
{
    /// <summary>
    /// Handles a query.
    /// </summary>
    /// <param name="request">The request to handle.</param>
    /// <param name="cancellationToken">The cancellation token when requesting the cancellation of the execution.</param>
    /// <returns>The request result.</returns>
    Task<TResult> HandleAsync(TRequest request, CancellationToken cancellationToken);
}

