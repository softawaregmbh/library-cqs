using softaware.Cqs.Decorators.Validation;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Provides extension methods to add data annotations decorators.
/// </summary>
public static class SoftawareCqsValidationDecoratorBuilderExtensions
{
    /// <summary>
    /// Registers the data annotations validation command and query decorators.
    /// </summary>
    /// <param name="decoratorBuilder">The CQS decorator builder.</param>
    /// <returns>The CQS decorator builder.</returns>
    public static SoftawareCqsDecoratorBuilder AddDataAnnotationsValidationDecorators(this SoftawareCqsDecoratorBuilder decoratorBuilder)
    {
        decoratorBuilder.Services.AddSingleton<IValidator>(new DataAnnotationsValidator());

        decoratorBuilder.AddCommandHandlerDecorator(typeof(ValidationCommandHandlerDecorator<>));
        decoratorBuilder.AddQueryHandlerDecorator(typeof(ValidationQueryHandlerDecorator<,>));

        return decoratorBuilder;
    }
}
