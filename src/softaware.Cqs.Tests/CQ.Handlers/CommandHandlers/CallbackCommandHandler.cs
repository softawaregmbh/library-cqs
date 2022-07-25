using softaware.Cqs.Tests.CQ.Contract.Commands;

namespace softaware.Cqs.Tests.CQ.Handlers.CommandHandlers;

internal class CallbackCommandHandler : ICommandHandler<CallbackCommand>
{
    public Task HandleAsync(CallbackCommand command, CancellationToken cancellationToken)
    {
        command.Action();

        if (command.ShouldThrow)
        {
            throw new InvalidOperationException("We throw here for testing the rollback of transactions.");
        }

        return Task.CompletedTask;
    }
}
