using softaware.Cqs.Tests.CQ.Contract.Commands;
using softaware.Cqs.Tests.Fakes;

namespace softaware.Cqs.Tests.CQ.Handlers.CommandHandlers;

internal class CommandWithDependencyHandler(IDependency dependency) : IRequestHandler<CommandWithDependency, NoResult>
{
    public Task<NoResult> HandleAsync(CommandWithDependency command, CancellationToken cancellationToken)
    {
        dependency.SomeMethod();

        return NoResult.CompletedTask;
    }
}
