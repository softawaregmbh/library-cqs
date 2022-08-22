using softaware.Cqs.Tests.Fakes;

namespace softaware.Cqs.Tests.Decorators;

public class AccessControlCommandHandlerDecorator<TCommand, TResult> : IRequestHandler<TCommand, TResult>
    where TCommand : ICommand<TResult>, IAccessChecked
{
    private readonly IRequestHandler<TCommand, TResult> decoratee;

    public AccessControlCommandHandlerDecorator(IRequestHandler<TCommand, TResult> decoratee)
        => this.decoratee = decoratee;

    public async Task<TResult> HandleAsync(TCommand command, CancellationToken cancellationToken)
    {
        command.AccessCheckHasBeenEvaluated = true;
        return await this.decoratee.HandleAsync(command, cancellationToken);
    }
}
