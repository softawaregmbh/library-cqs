namespace softaware.Cqs.Tests.Decorators;

public class InvalidDecoratorWithoutConstructor<TRequest, TResult> : IRequestHandler<TRequest, TResult>
    where TRequest : IRequest<TResult>
{

    public Task<TResult> HandleAsync(TRequest request, CancellationToken cancellationToken) => Task.FromResult(default(TResult)!);
}
