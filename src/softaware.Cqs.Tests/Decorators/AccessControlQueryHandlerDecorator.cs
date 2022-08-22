using softaware.Cqs.Tests.Fakes;

namespace softaware.Cqs.Tests.Decorators;

public class AccessControlQueryHandlerDecorator<TQuery, TResult> : IRequestHandler<TQuery, TResult>
    where TQuery : IQuery<TResult>, IAccessChecked
{
    private readonly IRequestHandler<TQuery, TResult> decoratee;

    public AccessControlQueryHandlerDecorator(IRequestHandler<TQuery, TResult> decoratee)
        => this.decoratee = decoratee;

    public async Task<TResult> HandleAsync(TQuery query, CancellationToken cancellationToken)
    {
        query.AccessCheckHasBeenEvaluated = true;
        return await this.decoratee.HandleAsync(query, cancellationToken);
    }
}
