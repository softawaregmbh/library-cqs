using System.Linq.Expressions;
using softaware.Cqs;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Finds the matching <see cref="IRequestHandler{TRequest, TResult}"/> for a specified <see cref="IRequest{TResult}"/> and
/// calls <see cref="IRequestHandler{TRequest, TResult}.HandleAsync(TRequest, CancellationToken)"/> on that handler.
/// </summary>
public class DynamicRequestProcessor : IRequestProcessor
{
    private readonly IServiceProvider serviceProvider;

    /// <summary>
    /// Initializes a new instance of the <see cref="DynamicRequestProcessor"/> class.
    /// </summary>
    /// <param name="serviceProvider">The service provider.</param>
    public DynamicRequestProcessor(IServiceProvider serviceProvider) =>
        this.serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));

    /// <summary>
    /// Finds the matching <see cref="IRequestHandler{TRequest, TResult}"/> for a specified <see cref="IRequest{TResult}"/> and
    /// calls <see cref="IRequestHandler{TRequest, TResult}.HandleAsync(TRequest, CancellationToken)"/> on that handler.
    /// </summary>
    /// <param name="request">The request to execute.</param>
    /// <param name="cancellationToken">The cancellation token for requesting the cancellation of the execution.</param>
    /// <returns>The result.</returns>
    public async Task<TResult> ExecuteAsync<TResult>(IRequest<TResult> request, CancellationToken cancellationToken = default)
    {
        var handlerType = typeof(IRequestHandler<,>).MakeGenericType(request.GetType(), typeof(TResult));
        var handler = this.serviceProvider.GetRequiredService(handlerType);

        // 'handler' is of type object and we cannot cast it to the
        // correct interface type because of the generic parameters.
        // 
        // So we build the following lambda expression and invoke it:
        //
        // () => handler.HandleAsync(request, cancellationToken)

        return await Expression.Lambda<Func<Task<TResult>>>(
            body: Expression.Call(
                instance: Expression.Constant(handler, handlerType),
                methodName: nameof(IRequestHandler<IRequest<TResult>, TResult>.HandleAsync),
                typeArguments: null,
                // arguments:
                Expression.Constant(request),
                Expression.Constant(cancellationToken))).Compile(true).Invoke();
    }
}
