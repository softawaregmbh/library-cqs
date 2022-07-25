using softaware.Cqs.Tests.Fakes;

namespace softaware.Cqs.Tests.Decorators;

public class AccessControlCommandHandlerDecorator<TCommand> : ICommandHandler<TCommand>
    where TCommand : ICommand, IAccessChecked
{
    private readonly ICommandHandler<TCommand> decoratee;

    public AccessControlCommandHandlerDecorator(ICommandHandler<TCommand> decoratee)
        => this.decoratee = decoratee;

    public async Task HandleAsync(TCommand command, CancellationToken cancellationToken)
    {
        command.AccessCheckHasBeenEvaluated = true;
        await this.decoratee.HandleAsync(command, cancellationToken);
    }
}
