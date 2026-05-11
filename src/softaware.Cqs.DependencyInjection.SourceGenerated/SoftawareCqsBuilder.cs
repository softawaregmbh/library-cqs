using softaware.Cqs.DependencyInjection;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Provides methods for configuring the softaware CQS infrastructure with source-generated registrations.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="SoftawareCqsBuilder"/> class.
/// </remarks>
/// <param name="services">The service collection.</param>
public class SoftawareCqsBuilder(IServiceCollection services)
{
    /// <summary>
    /// The service collection.
    /// </summary>
    public IServiceCollection Services { get; } = services ?? throw new ArgumentNullException(nameof(services));

    /// <summary>
    /// Configures decorators for the softaware CQS infrastructure.
    /// </summary>
    /// <remarks>
    /// Decorators are applied in reverse order. This means decorators which are registered last will be executed first.
    /// Decorators which are registered earlier will be executed "closer" to the actual handler.
    ///
    /// The source generator reads all decorator types from the syntax tree at compile time.
    /// At runtime, this method executes the lambda to record which decorators were actually requested,
    /// enabling conditional registration (decorators inside <c>if</c> blocks).
    /// </remarks>
    /// <param name="softawareCqsDecoratorBuilderAction">Provides an action to configure decorators.</param>
    /// <returns>The CQS builder.</returns>
    public SoftawareCqsBuilder AddDecorators(Action<SoftawareCqsDecoratorBuilder> softawareCqsDecoratorBuilderAction)
    {
        var builder = new SoftawareCqsDecoratorBuilder();
        softawareCqsDecoratorBuilderAction(builder);

        // Replace the default empty registry with one containing the actually-enabled decorators.
        // This allows conditional decorators (inside if/switch blocks) to work correctly:
        // the generated factory lambdas check registry.IsEnabled() at resolution time.
        var descriptor = new ServiceDescriptor(
            typeof(CqsDecoratorRegistry),
            new CqsDecoratorRegistry(builder.EnabledDecorators));
        Extensions.ServiceCollectionDescriptorExtensions.RemoveAll<CqsDecoratorRegistry>(this.Services);
        this.Services.Add(descriptor);

        return this;
    }
}
