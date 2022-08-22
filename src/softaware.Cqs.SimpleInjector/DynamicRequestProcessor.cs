using System.Linq.Expressions;
using SimpleInjector;

namespace softaware.Cqs.SimpleInjector;

/// <summary>
/// Finds the matching <see cref="IRequestHandler{TRequest, TResult}"/> for a specified <see cref="IRequest{TResult}"/> and
/// calls <see cref="IRequestHandler{TRequest, TResult}.HandleAsync(TRequest, CancellationToken)"/> on that handler.
/// </summary>
public class DynamicRequestProcessor : IRequestProcessor
{
    private readonly Container container;

    /// <summary>
    /// Initializes a new instance of the <see cref="DynamicRequestProcessor"/> class.
    /// </summary>
    /// <param name="container">The container.</param>
    public DynamicRequestProcessor(Container container) =>
        this.container = container ?? throw new ArgumentNullException(nameof(container));

    /// <inheritdoc />
    public async Task<TResult> HandleAsync<TResult>(IRequest<TResult> request, CancellationToken cancellationToken)
    {
        var handlerType = typeof(IRequestHandler<,>).MakeGenericType(request.GetType(), typeof(TResult));
        var handler = this.container.GetInstance(handlerType);

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
