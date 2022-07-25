using Microsoft.Extensions.DependencyInjection;

namespace softaware.Cqs.DependencyInjection;

/// <summary>
/// Finds the matching <see cref="ICommandHandler{TCommand}"/> for a specified <see cref="ICommand"/> and
/// calls <see cref="ICommandHandler{TCommand}.HandleAsync(TCommand, CancellationToken)"/> for that command handler.
/// </summary>
public class DynamicCommandProcessor : ICommandProcessor
{
    private readonly IServiceProvider serviceProvider;

    /// <summary>
    /// Initializes a new instance of the <see cref="DynamicCommandProcessor"/> class.
    /// </summary>
    /// <param name="serviceProvider">The service provider.</param>
    public DynamicCommandProcessor(IServiceProvider serviceProvider)
    {
        this.serviceProvider = serviceProvider;
    }

    /// <summary>
    /// Finds the matching <see cref="ICommandHandler{TCommand}"/> for a specified <see cref="ICommand"/> and
    /// calls <see cref="ICommandHandler{TCommand}.HandleAsync(TCommand, CancellationToken)"/> for that command handler.
    /// </summary>
    /// <param name="command">The command to execute.</param>
    /// <param name="cancellationToken">The optional cancellation token when requesting the cancellation of the execution.</param>
    public Task ExecuteAsync(ICommand command, CancellationToken cancellationToken = default)
    {
        var handlerType = typeof(ICommandHandler<>).MakeGenericType(command.GetType());

        dynamic handler = this.serviceProvider.GetRequiredService(handlerType);

        return handler.HandleAsync((dynamic)command, cancellationToken);
    }
}
