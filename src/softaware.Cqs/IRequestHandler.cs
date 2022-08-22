namespace softaware.Cqs;

/// <summary>
/// Interface for handlers and decorators of the given <typeparamref name="TRequest"/>.
/// </summary>
/// <typeparam name="TRequest">The type of the request to handle.</typeparam>
/// <typeparam name="TResult">The type of the result.</typeparam>
public interface IRequestHandler<TRequest, TResult>
    where TRequest : IRequest<TResult>
{
    /// <summary>
    /// Handles a <typeparamref name="TRequest"/>.
    /// </summary>
    /// <param name="request">The request to handle.</param>
    /// <param name="cancellationToken">The cancellation token when requesting the cancellation of the execution.</param>
    /// <returns>The request result.</returns>
    Task<TResult> HandleAsync(TRequest request, CancellationToken cancellationToken);
}

