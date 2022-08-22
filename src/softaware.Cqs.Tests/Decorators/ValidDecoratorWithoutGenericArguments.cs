using softaware.Cqs.Tests.CQ.Contract.Commands;

namespace softaware.Cqs.Tests.Decorators;

public class ValidDecoratorWithoutGenericArguments : IRequestHandler<SimpleCommand, NoResult>
{
    private readonly IRequestHandler<SimpleCommand, NoResult> decoratee;

    public ValidDecoratorWithoutGenericArguments(IRequestHandler<SimpleCommand, NoResult> decoratee) =>
        this.decoratee = decoratee ?? throw new ArgumentNullException(nameof(decoratee));

    public async Task<NoResult> HandleAsync(SimpleCommand request, CancellationToken cancellationToken)
    {
        request.Value++;
        return await this.decoratee.HandleAsync(request, cancellationToken);
    }
}
