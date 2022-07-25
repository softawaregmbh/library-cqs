using softaware.Cqs;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Finds the matching <see cref="IQueryHandler{TQuery, TResult}"/> for a specified <see cref="IQuery{TResult}"/> and
/// calls <see cref="IQueryHandler{TQuery, TResult}.HandleAsync(TQuery, CancellationToken)"/> for that query handler.
/// </summary>
public class DynamicQueryProcessor : IQueryProcessor
{
    private readonly IServiceProvider serviceProvider;

    /// <summary>
    /// Initializes a new instance of the <see cref="DynamicQueryProcessor"/> class.
    /// </summary>
    /// <param name="serviceProvider">The service provider.</param>
    public DynamicQueryProcessor(IServiceProvider serviceProvider)
    {
        this.serviceProvider = serviceProvider;
    }

    /// <summary>
    /// Finds the matching <see cref="IQueryHandler{TQuery, TResult}"/> for a specified <see cref="IQuery{TResult}"/> and
    /// calls <see cref="IQueryHandler{TQuery, TResult}.HandleAsync(TQuery, CancellationToken)"/> for that query handler.
    /// </summary>
    /// <param name="query">The query to execute.</param>
    /// <param name="cancellationToken">The optional cancellation token when requesting the cancellation of the execution.</param>
    /// <returns>The query result.</returns>
    public async Task<TResult> ExecuteAsync<TResult>(IQuery<TResult> query, CancellationToken cancellationToken = default)
    {
        var handlerType = typeof(IQueryHandler<,>).MakeGenericType(query.GetType(), typeof(TResult));

        dynamic handler = this.serviceProvider.GetRequiredService(handlerType);

        return await handler.HandleAsync((dynamic)query, cancellationToken);
    }
}
