using System.Diagnostics;

namespace softaware.Cqs.Decorators.OpenTelemetry;

/// <summary>
/// Creates an OpenTelemetry <see cref="Activity"/> (span) for every request handler.
/// This creates a CQS "call stack" in distributed traces.
/// </summary>
/// <remarks>
/// Activities are created from <see cref="CqsActivitySource.ActivitySource"/>.
/// To collect them, enable the source by calling
/// <c>AddSource(CqsActivitySource.Name)</c> (i.e. <c>AddSource("softaware.Cqs")</c>)
/// on your <c>TracerProviderBuilder</c>.
/// </remarks>
public class ActivityRequestHandlerDecorator<TRequest, TResult>(IRequestHandler<TRequest, TResult> decoratee) : IRequestHandler<TRequest, TResult>
    where TRequest : IRequest<TResult>
{
    public Task<TResult> HandleAsync(TRequest request, CancellationToken cancellationToken)
    {
        var requestType = request.GetType();

        using var activity = CqsActivitySource.ActivitySource.StartActivity(
            requestType.Name,
            ActivityKind.Internal);

        activity?.SetTag("cqs.request.type", requestType.FullName);

        return decoratee.HandleAsync(request, cancellationToken);
    }
}
