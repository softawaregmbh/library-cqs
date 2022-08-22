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
        var implementedInterfaceType = decoratorType.GetImplementedRequestHandlerInterfaceType();
        if (implementedInterfaceType == null)
        {
            throw new ArgumentException($"Type '{decoratorType}' cannot be used as decorator because it does not implement IRequestHandler<TRequest, TResult>.", nameof(decoratorType));
        }

        if (!decoratorType.IsDecorator())
        {
            throw new ArgumentException($"Type '{decoratorType}' cannot be used as decorator for '{implementedInterfaceType}' because it has no constructor parameter with this type.", nameof(decoratorType));
        }

        // We have to distinguish between generic IRequestHandler implementations
        // (e.g. class Decorator<TRequest, TResult>: IRequestHandler<TRequest, TResult>)
        // that should be applied to different request types (based on the generic
        // constraints) and non-generic implementations that target a specific request type
        // (e.g. class Decorator: IRequestHandler<ICommand, NoResult).
        // A mix of both is currently not supported.

        int genericParametersInImplementedInterface = implementedInterfaceType.GetGenericArguments().Count(t => t.IsGenericParameter);

        switch (genericParametersInImplementedInterface)
        {
            case 0:
                // register as IRequestHandler<ICommand, NoResult>
                this.Services.Decorate(implementedInterfaceType, decoratorType);
                break;

            case 2:
                // register as IRequestHandler<,>
                this.Services.Decorate(RequestHandlerTypeHelper.GenericRequestHandlerTypeDefinition, decoratorType);
                break;

            default:
                var message =
@"Sorry, partially closed decorators are not supported at the moment. You can achieve the same behavior by changing your decorator class definition to a generic type with two type arguments and applying type constraints:

For example:

class Decorator<TResult> : IRequestHandler<SomeRequest, TResult>
    where TRequest : IRequest<TResult>

can be changed to

class Decorator<TRequest, TResult> : IRequestHandler<TRequest, TResult>
    where TRequest : SomeRequest

Alternatively, you can use softaware.CQS.SimpleInjector instead of softaware.CQS.DependencyInjection.";
                throw new NotSupportedException(message);
        }

        return this;
    }
}
