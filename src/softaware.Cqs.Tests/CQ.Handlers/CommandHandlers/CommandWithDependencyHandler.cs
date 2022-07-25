using softaware.Cqs.Tests.CQ.Contract.Commands;
using softaware.Cqs.Tests.Fakes;

namespace softaware.Cqs.Tests.CQ.Handlers.CommandHandlers;

public class CommandWithDependencyHandler : ICommandHandler<CommandWithDependency>
{
    private readonly IDependency dependency;

    public CommandWithDependencyHandler(IDependency dependency)
        => this.dependency = dependency;

    public Task HandleAsync(CommandWithDependency command, CancellationToken cancellationToken)
    {
        this.dependency.SomeMethod();

        return Task.CompletedTask;
    }
}
