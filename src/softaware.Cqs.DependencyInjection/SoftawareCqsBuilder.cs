using System;
using softaware.Cqs;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// Provides methods for configuring the softaware CQS infrastructure.
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
        public SoftawareCqsBuilder(IServiceCollection services)
        {
            this.Services = services;
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
            var decoratorBuilder = new SoftawareCqsDecoratorBuilder(this.Services);
            softawareCqsDecoratorBuilderAction.Invoke(decoratorBuilder);

            // Register public decorators as last decorator if any decorators are registered.
            this.Services.Decorate(typeof(IQueryHandler<,>), typeof(PublicQueryHandlerDecorator<,>));
            this.Services.Decorate(typeof(ICommandHandler<>), typeof(PublicCommandHandlerDecorator<>));

            return this;
        }
    }
}
