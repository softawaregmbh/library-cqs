using System;
using SimpleInjector;

namespace softaware.Cqs
{
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
        {
            this.Container = container;
        }

        /// <summary>
        /// Adds a command handler decorator.
        /// </summary>
        /// <param name="decoratorType">Type type of the decorator. The decorator must implement <see cref="ICommandHandler{TCommand}"/>.</param>
        /// <returns></returns>
        public SoftawareCqsDecoratorBuilder AddCommandHandlerDecorator(Type decoratorType)
        {
            this.Container.RegisterDecorator(typeof(ICommandHandler<>), decoratorType);
            return this;
        }

        /// <summary>
        /// Adds a query handler decorator.
        /// </summary>
        /// <param name="decoratorType">Type type of the decorator. The decorator must implement <see cref="IQueryHandler{TQuery, TResult}"/>.</param>
        /// <returns></returns>
        public SoftawareCqsDecoratorBuilder AddQueryHandlerDecorator(Type decoratorType)
        {
            this.Container.RegisterDecorator(typeof(IQueryHandler<,>), decoratorType);
            return this;
        }
    }
}
