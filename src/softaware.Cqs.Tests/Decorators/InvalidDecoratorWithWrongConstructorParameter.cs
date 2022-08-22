namespace softaware.Cqs.Tests.Decorators;

public class InvalidDecoratorWithWrongConstructorParameter<TRequest> : IRequestHandler<TRequest, int>
    where TRequest : IRequest<int>
{

    public InvalidDecoratorWithWrongConstructorParameter(IRequestHandler<IRequest<double>, double> _)
    {
    }

    public Task<int> HandleAsync(TRequest request, CancellationToken cancellationToken) => Task.FromResult(0);
}
