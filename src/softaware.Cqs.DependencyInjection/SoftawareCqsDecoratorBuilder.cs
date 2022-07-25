using softaware.Cqs;

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
    public SoftawareCqsDecoratorBuilder(IServiceCollection services)
    {
        this.Services = services;
    }

    /// <summary>
    /// Adds a command handler decorator.
    /// </summary>
    /// <param name="decoratorType">Type type of the decorator. The decorator must implement <see cref="ICommandHandler{TCommand}"/>.</param>
    /// <returns></returns>
    public SoftawareCqsDecoratorBuilder AddCommandHandlerDecorator(Type decoratorType)
    {
        this.Services.Decorate(typeof(ICommandHandler<>), decoratorType);
        return this;
    }

    /// <summary>
    /// Adds a query handler decorator.
    /// </summary>
    /// <param name="decoratorType">Type type of the decorator. The decorator must implement <see cref="IQueryHandler{TQuery, TResult}"/>.</param>
    /// <returns></returns>
    public SoftawareCqsDecoratorBuilder AddQueryHandlerDecorator(Type decoratorType)
    {
        this.Services.Decorate(typeof(IQueryHandler<,>), decoratorType);
        return this;
    }
}
