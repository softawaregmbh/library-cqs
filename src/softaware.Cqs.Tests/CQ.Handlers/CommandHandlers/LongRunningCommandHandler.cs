using softaware.Cqs.Tests.CQ.Contract.Commands;

namespace softaware.Cqs.Tests.CQ.Handlers.CommandHandlers;

internal class LongRunningCommandHandler : IRequestHandler<LongRunningCommand, NoResult>
{
    public Task HandleAsync(LongRunningCommand command)
    {
        return this.HandleAsync(command, default);
    }

    public async Task<NoResult> HandleAsync(LongRunningCommand command, CancellationToken cancellationToken)
    {
        await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);
        return NoResult.Value;
    }
}
