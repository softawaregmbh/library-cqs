namespace softaware.Cqs;

/// <summary>
/// Processes a specified request.
/// </summary>
public interface IRequestProcessor
{
    /// <summary>
    /// Executes the specified request.
    /// </summary>
    /// <param name="request">The request to execute.</param>
    /// <param name="cancellationToken">The optional cancellation token for requesting cancellation of the request handling.</param>
    /// <returns>The result.</returns>
    Task<TResult> ExecuteAsync<TResult>(IRequest<TResult> request, CancellationToken cancellationToken = default);
}
