using softaware.Cqs.Tests.CQ.Contract.Commands;

namespace softaware.Cqs.Tests.CQ.Handlers.CommandHandlers;

public class AccessCheckedCommandHandler : ICommandHandler<AccessCheckedCommand>
{
    public Task HandleAsync(AccessCheckedCommand command, CancellationToken cancellationToken)
    {
        // nothing to do.
        return Task.CompletedTask;
    }
}
