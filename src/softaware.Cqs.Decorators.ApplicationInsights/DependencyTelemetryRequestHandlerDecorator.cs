using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;

namespace softaware.Cqs.Decorators.ApplicationInsights;

/// <summary>
/// Creates an application insights dependency telementry for every request handler.
/// This creates a CQS "call stack" in Application Insights end-to-end transaction details.
/// </summary>
/// <remarks>
/// Requires the <see cref="TelemetryClient"/> to be registered in the dependency injection container.
/// </remarks>
public class DependencyTelemetryRequestHandlerDecorator<TRequest, TResult> : IRequestHandler<TRequest, TResult>
    where TRequest : IRequest<TResult>
{
    private readonly TelemetryClient telemetryClient;
    private readonly IRequestHandler<TRequest, TResult> decoratee;

    public DependencyTelemetryRequestHandlerDecorator(
        TelemetryClient telemetryClient,
        IRequestHandler<TRequest, TResult> decoratee)
    {
        this.telemetryClient = telemetryClient ?? throw new ArgumentNullException(nameof(telemetryClient));
        this.decoratee = decoratee ?? throw new ArgumentNullException(nameof(decoratee));
    }

    public Task<TResult> HandleAsync(TRequest request, CancellationToken cancellationToken)
    {
        using var operation = this.telemetryClient.StartOperation<DependencyTelemetry>(request.GetType().Name);
        operation.Telemetry.Type = "CQS";

        try
        {
            return this.decoratee.HandleAsync(request, cancellationToken);
        }
        finally
        {
            this.telemetryClient.StopOperation(operation);
        }
    }
}
