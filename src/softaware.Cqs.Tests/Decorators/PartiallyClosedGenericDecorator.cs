using softaware.Cqs.Tests.CQ.Contract.Commands;

namespace softaware.Cqs.Tests.Decorators;

public class PartiallyClosedGenericDecorator<TRequest> : IRequestHandler<TRequest, NoResult>
    where TRequest : SimpleCommand
{
    private readonly IRequestHandler<TRequest, NoResult> decoratee;

    public PartiallyClosedGenericDecorator(IRequestHandler<TRequest, NoResult> decoratee) =>
        this.decoratee = decoratee ?? throw new ArgumentNullException(nameof(decoratee));

    public async Task<NoResult> HandleAsync(TRequest request, CancellationToken cancellationToken)
    {
        request.Value++;
        return await this.decoratee.HandleAsync(request, cancellationToken);
    }
}
