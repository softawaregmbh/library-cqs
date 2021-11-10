using softaware.Cqs;
using System;

namespace SimpleInjector
{
    /// <summary>
    /// Provides methods for configuring the softaware CQS infrastructure.
    /// </summary>
    public class SoftawareCqsBuilder
    {
        /// <summary>
        /// The SimpleInjector container.
        /// </summary>
        public Container Container { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="SoftawareCqsBuilder"/> class.
        /// </summary>
        /// <param name="container">The SimpleInjector container.</param>
        public SoftawareCqsBuilder(Container container)
        {
            this.Container = container;
        }

        /// <summary>
        /// Enables decorators for the softaware CQS infrastructure.
        /// </summary>
        /// <remarks>
        /// Decorators are applied in reverse order. This means decorators which are registered last will be executed first.
        /// Decorators which are registered earlier will be executed "closer" to the actual handler.
        /// </remarks>
        /// <param name="softawareCqsDecoratorBuilderAction">Provides an action to configure decorators.</param>
        /// <returns>The CQS builder.</returns>
        public SoftawareCqsBuilder AddDecorators(Action<SoftawareCqsDecoratorBuilder> softawareCqsDecoratorBuilderAction)
        {
            var decoratorBuilder = new SoftawareCqsDecoratorBuilder(this.Container);
            softawareCqsDecoratorBuilderAction.Invoke(decoratorBuilder);

            // Register public decorators as last decorator if any decorators are registered.
            this.Container.RegisterDecorator(typeof(IQueryHandler<,>), typeof(PublicQueryHandlerDecorator<,>));
            this.Container.RegisterDecorator(typeof(ICommandHandler<>), typeof(PublicCommandHandlerDecorator<>));

            return this;
        }
    }
}
