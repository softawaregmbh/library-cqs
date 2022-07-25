using softaware.Cqs.Tests.CQ.Contract.Commands;

namespace softaware.Cqs.Tests.CQ.Handlers.CommandHandlers;

internal class StartAndEndDateCommandHandler : ICommandHandler<StartAndEndDateCommand>
{
    public Task HandleAsync(StartAndEndDateCommand command, CancellationToken cancellationToken)
    {
        // Just for testing: Change the value and assert the change in test afterwards.
        command.CommandExecuted = true;

        return Task.CompletedTask;
    }
}
