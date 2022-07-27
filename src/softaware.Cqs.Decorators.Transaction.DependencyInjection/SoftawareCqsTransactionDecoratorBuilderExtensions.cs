using softaware.Cqs.Decorators.Transaction;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Provides extension methods to add transaction aware decorators.
/// </summary>
public static class SoftawareCqsTransactionDecoratorBuilderExtensions
{
    /// <summary>
    /// Registers the transaction aware command handler decorator.
    /// </summary>
    /// <param name="decoratorBuilder">The CQS decorator builder.</param>
    /// <returns>The CQS decorator builder.</returns>
    public static SoftawareCqsDecoratorBuilder AddTransactionCommandHandlerDecorator(this SoftawareCqsDecoratorBuilder decoratorBuilder)
    {
        decoratorBuilder.AddRequestHandlerDecorator(typeof(TransactionAwareCommandHandlerDecorator<,>));

        return decoratorBuilder;
    }

    /// <summary>
    /// Registers the transaction aware query handler decorator.
    /// </summary>
    /// <param name="decoratorBuilder">The CQS decorator builder.</param>
    /// <returns>The CQS decorator builder.</returns>
    public static SoftawareCqsDecoratorBuilder AddTransactionQueryHandlerDecorator(this SoftawareCqsDecoratorBuilder decoratorBuilder)
    {
        decoratorBuilder.AddRequestHandlerDecorator(typeof(TransactionAwareQueryHandlerDecorator<,>));

        return decoratorBuilder;
    }
}
