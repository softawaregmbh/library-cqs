using softaware.Cqs.Benchmarks.Contracts;

namespace softaware.Cqs.Benchmarks.Decorators;

/// <summary>
/// Applies to ALL requests (where TRequest : IRequest&lt;TResult&gt;).
/// Simulates a logging/telemetry decorator.
/// </summary>
public class LoggingDecorator<TRequest, TResult> : IRequestHandler<TRequest, TResult>
    where TRequest : IRequest<TResult>
{
    private readonly IRequestHandler<TRequest, TResult> decoratee;

    public LoggingDecorator(IRequestHandler<TRequest, TResult> decoratee)
        => this.decoratee = decoratee;

    public Task<TResult> HandleAsync(TRequest request, CancellationToken ct)
        => this.decoratee.HandleAsync(request, ct);
}

/// <summary>
/// Applies only to COMMANDS (where TRequest : ICommand&lt;TResult&gt;).
/// Simulates a transaction decorator.
/// </summary>
public class TransactionDecorator<TRequest, TResult> : IRequestHandler<TRequest, TResult>
    where TRequest : ICommand<TResult>
{
    private readonly IRequestHandler<TRequest, TResult> decoratee;

    public TransactionDecorator(IRequestHandler<TRequest, TResult> decoratee)
        => this.decoratee = decoratee;

    public Task<TResult> HandleAsync(TRequest request, CancellationToken ct)
        => this.decoratee.HandleAsync(request, ct);
}

/// <summary>
/// Applies only to QUERIES (where TRequest : IQuery&lt;TResult&gt;).
/// Simulates a caching decorator.
/// </summary>
public class CachingDecorator<TRequest, TResult> : IRequestHandler<TRequest, TResult>
    where TRequest : IQuery<TResult>
{
    private readonly IRequestHandler<TRequest, TResult> decoratee;

    public CachingDecorator(IRequestHandler<TRequest, TResult> decoratee)
        => this.decoratee = decoratee;

    public Task<TResult> HandleAsync(TRequest request, CancellationToken ct)
        => this.decoratee.HandleAsync(request, ct);
}

/// <summary>
/// Applies only to requests implementing IAccessChecked (multiple interface constraints).
/// </summary>
public class AccessCheckDecorator<TRequest, TResult> : IRequestHandler<TRequest, TResult>
    where TRequest : IRequest<TResult>, IAccessChecked
{
    private readonly IRequestHandler<TRequest, TResult> decoratee;

    public AccessCheckDecorator(IRequestHandler<TRequest, TResult> decoratee)
        => this.decoratee = decoratee;

    public Task<TResult> HandleAsync(TRequest request, CancellationToken ct)
    {
        request.AccessCheckEvaluated = true;
        return this.decoratee.HandleAsync(request, ct);
    }
}

/// <summary>
/// Applies only to prioritized commands (two constraints: ICommand + IPrioritized).
/// </summary>
public class PriorityDecorator<TRequest, TResult> : IRequestHandler<TRequest, TResult>
    where TRequest : ICommand<TResult>, IPrioritized
{
    private readonly IRequestHandler<TRequest, TResult> decoratee;

    public PriorityDecorator(IRequestHandler<TRequest, TResult> decoratee)
        => this.decoratee = decoratee;

    public Task<TResult> HandleAsync(TRequest request, CancellationToken ct)
        => this.decoratee.HandleAsync(request, ct);
}
