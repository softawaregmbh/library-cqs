using System;
using SimpleInjector;
using softaware.Cqs.SimpleInjector;

namespace softaware.Cqs
{
    public class SoftawareCqsBuilder
    {
        public Container Container { get; }

        public SoftawareCqsBuilder(Container container)
        {
            this.Container = container;
        }

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
