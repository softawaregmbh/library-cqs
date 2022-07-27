namespace softaware.Cqs.Decorators.UsageAware;

/// <summary>
/// A decorator for tracking command executions with UsageAware.
/// </summary>
/// <typeparam name="TRequest">The request to execute.</typeparam>
/// <typeparam name="TResult">The type of the result.</typeparam>
public class UsageAwareRequestHandlerDecorator<TRequest, TResult> : IRequestHandler<TRequest, TResult>
    where TRequest : IRequest<TResult>
{
    private readonly UsageAwareLogger<TRequest, TResult> logger;
    private readonly IRequestHandler<TRequest, TResult> decoratee;

    public UsageAwareRequestHandlerDecorator(
        UsageAwareLogger<TRequest, TResult> logger,
        IRequestHandler<TRequest, TResult> decoratee)
    {
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        this.decoratee = decoratee ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<TResult> HandleAsync(TRequest command, CancellationToken cancellationToken)
    {
        var result = default(TResult);
        await this.logger.TimeAndLogAsync(async () => result = await this.decoratee.HandleAsync(command, cancellationToken));
        return result!;
    }
}
