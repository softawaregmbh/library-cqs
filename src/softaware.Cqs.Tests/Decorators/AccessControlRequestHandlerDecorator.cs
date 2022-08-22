using softaware.Cqs.Tests.Fakes;

namespace softaware.Cqs.Tests.Decorators;

public class AccessControlRequestHandlerDecorator<TRequest, TResult> : IRequestHandler<TRequest, TResult>
    where TRequest : IRequest<TResult>, IAccessChecked
{
    private readonly IRequestHandler<TRequest, TResult> decoratee;

    public AccessControlRequestHandlerDecorator(IRequestHandler<TRequest, TResult> decoratee)
        => this.decoratee = decoratee;

    public async Task<TResult> HandleAsync(TRequest request, CancellationToken cancellationToken)
    {
        request.AccessCheckHasBeenEvaluated = true;
        return await this.decoratee.HandleAsync(request, cancellationToken);
    }
}
