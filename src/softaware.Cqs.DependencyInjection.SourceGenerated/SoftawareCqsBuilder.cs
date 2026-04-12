namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Provides methods for configuring the softaware CQS infrastructure with source-generated registrations.
/// </summary>
public class SoftawareCqsBuilder
{
    /// <summary>
    /// The service collection.
    /// </summary>
    public IServiceCollection Services { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="SoftawareCqsBuilder"/> class.
    /// </summary>
    /// <param name="services">The service collection.</param>
    public SoftawareCqsBuilder(IServiceCollection services) =>
        this.Services = services ?? throw new ArgumentNullException(nameof(services));

    /// <summary>
    /// Configures decorators for the softaware CQS infrastructure.
    /// </summary>
    /// <remarks>
    /// Decorators are applied in reverse order. This means decorators which are registered last will be executed first.
    /// Decorators which are registered earlier will be executed "closer" to the actual handler.
    ///
    /// At runtime this method is a no-op. The source generator reads the decorator configuration
    /// from the syntax tree at compile time and generates explicit decorator chains.
    /// </remarks>
    /// <param name="softawareCqsDecoratorBuilderAction">Provides an action to configure decorators.</param>
    /// <returns>The CQS builder.</returns>
    public SoftawareCqsBuilder AddDecorators(Action<SoftawareCqsDecoratorBuilder> softawareCqsDecoratorBuilderAction)
    {
        // No-op at runtime. The source generator reads this call syntactically at compile time.
        return this;
    }
}
