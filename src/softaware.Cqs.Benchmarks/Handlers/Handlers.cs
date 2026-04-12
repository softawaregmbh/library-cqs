using softaware.Cqs.Benchmarks.Contracts.Commands;
using softaware.Cqs.Benchmarks.Contracts.Queries;

namespace softaware.Cqs.Benchmarks.Handlers;

internal class SimpleCommandHandler : IRequestHandler<SimpleCommand, NoResult>
{
    public Task<NoResult> HandleAsync(SimpleCommand command, CancellationToken ct)
    {
        command.Value += 1;
        return NoResult.CompletedTask;
    }
}

internal class AccessCheckedCommandHandler : IRequestHandler<AccessCheckedCommand, NoResult>
{
    public Task<NoResult> HandleAsync(AccessCheckedCommand command, CancellationToken ct)
        => NoResult.CompletedTask;
}

internal class HighPriorityCommandHandler : IRequestHandler<HighPriorityCommand, NoResult>
{
    public Task<NoResult> HandleAsync(HighPriorityCommand command, CancellationToken ct)
        => NoResult.CompletedTask;
}

internal class CommandWithResultHandler : IRequestHandler<CommandWithResult, int>
{
    public Task<int> HandleAsync(CommandWithResult command, CancellationToken ct)
        => Task.FromResult(command.Input * 2);
}

internal class GetSquareHandler : IRequestHandler<GetSquare, int>
{
    public Task<int> HandleAsync(GetSquare query, CancellationToken ct)
        => Task.FromResult(query.Value * query.Value);
}

internal class GetGreetingHandler : IRequestHandler<GetGreeting, string>
{
    public Task<string> HandleAsync(GetGreeting query, CancellationToken ct)
        => Task.FromResult($"Hello, {query.Name}!");
}

internal class AccessCheckedQueryHandler : IRequestHandler<AccessCheckedQuery, bool>
{
    public Task<bool> HandleAsync(AccessCheckedQuery query, CancellationToken ct)
        => Task.FromResult(true);
}
