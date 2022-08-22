using softaware.Cqs.Tests.CQ.Contract.Commands;

namespace softaware.Cqs.Tests.CQ.Handlers.CommandHandlers;

internal class StartAndEndDateCommandHandler : IRequestHandler<StartAndEndDateCommand, NoResult>
{
    public Task<NoResult> HandleAsync(StartAndEndDateCommand command, CancellationToken cancellationToken)
    {
        // Just for testing: Change the value and assert the change in test afterwards.
        command.CommandExecuted = true;

        return NoResult.CompletedTask;
    }
}
