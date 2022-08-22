namespace softaware.Cqs.Decorators.Validation;

/// <summary>
/// A decorator for validating the specified request. Uses the contructor injected <see cref="IValidator"/> for validating the query.
/// </summary>
/// <typeparam name="TRequest">The type of the request to execute.</typeparam>
/// <typeparam name="TResult">The type of the result.</typeparam>
public class ValidationRequestHandlerDecorator<TRequest, TResult> : IRequestHandler<TRequest, TResult>
    where TRequest : IRequest<TResult>
{
    private readonly IValidator validator;
    private readonly IRequestHandler<TRequest, TResult> decoratee;

    public ValidationRequestHandlerDecorator(
        IValidator validator,
        IRequestHandler<TRequest, TResult> decoratee)
    {
        this.validator = validator;
        this.decoratee = decoratee;
    }

    public Task<TResult> HandleAsync(TRequest request, CancellationToken cancellationToken)
    {
        if (request == null)
        {
            throw new ArgumentNullException(nameof(request));
        }

        this.validator.ValidateObject(request);
        return this.decoratee.HandleAsync(request, cancellationToken);
    }
}
