using softaware.Cqs.Tests.CQ.Contract.Commands;

namespace softaware.Cqs.Tests.CQ.Handlers.CommandHandlers;

public class CommandWithResultHandler : IRequestHandler<CommandWithResult, Guid>
{
    public Task<Guid> HandleAsync(CommandWithResult request, CancellationToken cancellationToken) =>
        Task.FromResult(Guid.Empty);
}
