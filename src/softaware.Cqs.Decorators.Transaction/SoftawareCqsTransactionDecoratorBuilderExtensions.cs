using softaware.Cqs.Decorators.Transaction;

namespace softaware.Cqs
{
    public static class SoftawareCqsTransactionDecoratorBuilderExtensions
    {
        public static SoftawareCqsDecoratorBuilder AddTransactionCommandHandlerDecorator(this SoftawareCqsDecoratorBuilder decoratorBuilder)
        {
            decoratorBuilder.AddCommandHandlerDecorator(typeof(TransactionAwareCommandHandlerDecorator<>));

            return decoratorBuilder;
        }

        public static SoftawareCqsDecoratorBuilder AddTransactionQueryHandlerDecorator(this SoftawareCqsDecoratorBuilder decoratorBuilder)
        {
            decoratorBuilder.AddCommandHandlerDecorator(typeof(TransactionAwareQueryHandlerDecorator<,>));

            return decoratorBuilder;
        }
    }
}
