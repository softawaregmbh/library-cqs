using softaware.Cqs.Tests.CQ.Contract.Commands;

namespace softaware.Cqs.Tests.CQ.Handlers.CommandHandlers;

public class AccessCheckedCommandHandler : IRequestHandler<AccessCheckedCommand, NoResult>
{
    public Task<NoResult> HandleAsync(AccessCheckedCommand command, CancellationToken cancellationToken)
    {
        // nothing to do.
        return NoResult.CompletedTask;
    }
}
