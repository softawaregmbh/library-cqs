#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
using SimpleInjector;

namespace softaware.Cqs.SimpleInjector;

/// <summary>
/// Finds the matching <see cref="IQueryHandler{TQuery, TResult}"/> for a specified <see cref="IQuery{TResult}"/> and
/// calls <see cref="IQueryHandler{TQuery, TResult}.HandleAsync(TQuery, CancellationToken)"/> for that query handler.
/// </summary>
public class DynamicQueryProcessor : IQueryProcessor
{
    private readonly Container container;

    public DynamicQueryProcessor(Container container)
    {
        this.container = container;
    }

    /// <summary>
    /// Finds the matching <see cref="IQueryHandler{TQuery, TResult}"/> for a specified <see cref="IQuery{TResult}"/> and
    /// calls <see cref="IQueryHandler{TQuery, TResult}.HandleAsync(TQuery, CancellationToken)"/> for that query handler.
    /// </summary>
    /// <param name="query">The query to execute.</param>
    /// <param name="cancellationToken">The optional cancellation token when requesting the cancellation of the execution.</param>
    /// <returns>The query result.</returns>
    public Task<TResult> ExecuteAsync<TResult>(IQuery<TResult> query, CancellationToken cancellationToken = default)
    {
        var handlerType = typeof(IQueryHandler<,>).MakeGenericType(query.GetType(), typeof(TResult));

        dynamic handler = this.container.GetInstance(handlerType);

        return handler.HandleAsync((dynamic)query, cancellationToken);
    }
}
