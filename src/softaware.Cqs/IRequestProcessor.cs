namespace softaware.Cqs;

/// <summary>
/// Processes a specified request.
/// </summary>
public interface IRequestProcessor
{
    /// <summary>
    /// Finds the matching <see cref="IRequestHandler{TRequest, TResult}"/> for a specified <see cref="IRequest{TResult}"/> and
    /// calls <see cref="IRequestHandler{TRequest, TResult}.HandleAsync(TRequest, CancellationToken)"/> on that handler.
    /// </summary>
    /// <param name="request">The request to execute.</param>
    /// <param name="cancellationToken">The cancellation token for requesting the cancellation of the execution.</param>
    /// <returns>The result.</returns>
    Task<TResult> HandleAsync<TResult>(IRequest<TResult> request, CancellationToken cancellationToken);
}
