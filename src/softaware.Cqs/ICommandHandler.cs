namespace softaware.Cqs;

/// <summary>
/// The command handler which handles the specified command.
/// </summary>
/// <typeparam name="TCommand">The type of the command to handle.</typeparam>
/// <typeparam name="TResult">The type of the command result.</typeparam>
public interface ICommandHandler<TCommand, TResult> : IRequestHandler<TCommand, TResult>
    where TCommand : ICommand<TResult>
{
}

/// <summary>
/// The command handler which handles the specified command.
/// </summary>
/// <typeparam name="TCommand">The type of the command to handle.</typeparam>
public interface ICommandHandler<TCommand> : IRequestHandler<TCommand, Void>
    where TCommand : ICommand
{
    /// <summary>
    /// Handles a command.
    /// </summary>
    /// <param name="command">The command to handle.</param>
    /// <param name="cancellationToken">The cancellation token when requesting the cancellation of the execution.</param>
    new Task HandleAsync(TCommand command, CancellationToken cancellationToken);

    async Task<Void> IRequestHandler<TCommand, Void>.HandleAsync(TCommand command, CancellationToken cancellationToken)
    {
        await this.HandleAsync(command, cancellationToken);
        return Void.Value;
    }
}
