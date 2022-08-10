using softaware.Cqs;
using softaware.Cqs.DependencyInjection;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Provides methods for configuring decorators for the softaware CQS infrastructure.
/// </summary>
public class SoftawareCqsDecoratorBuilder
{
    /// <summary>
    /// The service collection.
    /// </summary>
    public IServiceCollection Services { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="SoftawareCqsDecoratorBuilder"/> class.
    /// </summary>
    /// <param name="services">The service collection.</param>
    public SoftawareCqsDecoratorBuilder(IServiceCollection services) =>
        this.Services = services;

    /// <summary>
    /// Adds a request handler decorator.
    /// </summary>
    /// <param name="decoratorType">Type type of the decorator. The decorator must implement <see cref="IRequestHandler{TRequest, TResult}"/>.</param>
    public SoftawareCqsDecoratorBuilder AddRequestHandlerDecorator(Type decoratorType)
    {
        var requestHandlerType = typeof(IRequestHandler<,>);

        if (decoratorType.IsGenericType)
        {
            this.Services.Decorate(requestHandlerType, decoratorType);
        }
        else
        {
            // We have to register concrete decorators with concrete interface types:

            // class Decorator<ConcreteCommand, NoResult> : IRequestHandler<ConcreteCommand, NoResult>

            // has to be registered as IRequestHandler<ConcreteCommand, NoResult> and not IRequestHandler<,>.

            var interfaceType = TypeHelper.GetConcreteDecoratorInterface(decoratorType);
            this.Services.Decorate(interfaceType, decoratorType);
        }

        return this;
    }
}
