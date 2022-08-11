using softaware.Cqs.Tests.CQ.Contract.Commands;

namespace softaware.Cqs.Tests.Decorators;

public class ValidDecoratorWithGenericArguments<TRequest, TResult> : IRequestHandler<TRequest, TResult>
    where TRequest : SimpleCommand, IRequest<TResult>
{
    private readonly IRequestHandler<TRequest, TResult> decoratee;

    public ValidDecoratorWithGenericArguments(IRequestHandler<TRequest, TResult> decoratee) =>
        this.decoratee = decoratee ?? throw new ArgumentNullException(nameof(decoratee));

    public async Task<TResult> HandleAsync(TRequest request, CancellationToken cancellationToken)
    {
        request.Value++;
        return await this.decoratee.HandleAsync(request, cancellationToken);
    }
}
