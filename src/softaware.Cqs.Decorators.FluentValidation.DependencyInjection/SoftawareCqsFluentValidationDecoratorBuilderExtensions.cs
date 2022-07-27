using FluentValidation;
using softaware.Cqs;
using softaware.Cqs.Decorators.FluentValidation;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Provides extension methods to add FluentValidation decorators.
/// </summary>
public static class SoftawareCqsFluentValidationDecoratorBuilderExtensions
{
    /// <summary>
    /// Registers the fluent validation request handler decorators as well as all validators from the specified assemblies.
    /// </summary>
    /// <param name="decoratorBuilder">The CQS decorator builder.</param>
    /// <param name="validatorTypesBuilderAction">The types builder for registering assemblies from where to find <see cref="IValidator{T}"/> instances.</param>
    /// <returns>The CQS decorator builder.</returns>
    public static SoftawareCqsDecoratorBuilder AddFluentValidationDecorators(
        this SoftawareCqsDecoratorBuilder decoratorBuilder,
        Action<SoftawareCqsTypesBuilder> validatorTypesBuilderAction)
    {
        var typesBuilder = new SoftawareCqsTypesBuilder();
        validatorTypesBuilderAction.Invoke(typesBuilder);

        // Register all fluent validators which are available in the specified assemblies.
        decoratorBuilder.Services
            .Scan(scan => scan
                .FromAssemblies(typesBuilder.RegisteredAssemblies)
                    .AddClasses(classes => classes.AssignableTo(typeof(IValidator<>)))
                        .AsImplementedInterfaces()
                        .WithTransientLifetime());

        decoratorBuilder.AddRequestHandlerDecorator(typeof(FluentValidationRequestHandlerDecorator<,>));

        return decoratorBuilder;
    }
}
