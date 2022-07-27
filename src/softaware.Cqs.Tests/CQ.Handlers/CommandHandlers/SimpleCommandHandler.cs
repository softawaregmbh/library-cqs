using softaware.Cqs.Tests.CQ.Contract.Commands;

namespace softaware.Cqs.Tests.CQ.Handlers.CommandHandlers;

internal class SimpleCommandHandler : IRequestHandler<SimpleCommand, NoResult>
{
    public Task<NoResult> HandleAsync(SimpleCommand command, CancellationToken cancellationToken)
    {
        // Just for testing: Change the value and assert the change in test afterwards.
        command.Value += 1;
        return NoResult.Task;
    }
}
