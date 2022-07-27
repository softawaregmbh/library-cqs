using SimpleInjector;

namespace softaware.Cqs;

/// <summary>
/// Provides methods for configuring decorators for the softaware CQS infrastructure.
/// </summary>
public class SoftawareCqsDecoratorBuilder
{
    /// <summary>
    /// The SimpleInjector container.
    /// </summary>
    public Container Container { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="SoftawareCqsDecoratorBuilder"/> class.
    /// </summary>
    /// <param name="container">The SimpleInjector container.</param>
    public SoftawareCqsDecoratorBuilder(Container container)
        => this.Container = container ?? throw new ArgumentNullException(nameof(container));

    /// <summary>
    /// Adds a request handler decorator.
    /// </summary>
    /// <param name="decoratorType">Type type of the decorator. The decorator must implement <see cref="IRequestHandler{TRequest, TResult}"/>.</param>
    public SoftawareCqsDecoratorBuilder AddRequestHandlerDecorator(Type decoratorType)
    {
        this.Container.RegisterDecorator(typeof(IRequestHandler<,>), decoratorType);
        return this;
    }
}
