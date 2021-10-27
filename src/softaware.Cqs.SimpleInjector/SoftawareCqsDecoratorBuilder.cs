using System;
using SimpleInjector;

namespace softaware.Cqs
{
    public class SoftawareCqsDecoratorBuilder
    {
        public Container Container { get; }

        public SoftawareCqsDecoratorBuilder(Container container)
        {
            this.Container = container;
        }

        public SoftawareCqsDecoratorBuilder AddCommandHandlerDecorator(Type decoratorType)
        {
            this.Container.RegisterDecorator(typeof(ICommandHandler<>), decoratorType);
            return this;
        }

        public SoftawareCqsDecoratorBuilder AddQueryHandlerDecorator(Type decoratorType)
        {
            this.Container.RegisterDecorator(typeof(IQueryHandler<,>), decoratorType);
            return this;
        }
    }
}
