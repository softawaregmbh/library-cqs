using softaware.Cqs.Tests.CQ.Contract.Commands;
using softaware.Cqs.Tests.Fakes;

namespace softaware.Cqs.Tests.CQ.Handlers.CommandHandlers;

internal class CommandWithDependencyHandler : IRequestHandler<CommandWithDependency, NoResult>
{
    private readonly IDependency dependency;

    public CommandWithDependencyHandler(IDependency dependency)
        => this.dependency = dependency;

    public Task<NoResult> HandleAsync(CommandWithDependency command, CancellationToken cancellationToken)
    {
        this.dependency.SomeMethod();

        return NoResult.CompletedTask;
    }
}
