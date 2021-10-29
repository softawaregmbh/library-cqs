using System;
using FluentValidation;
using softaware.Cqs.Decorators.FluentValidation;

namespace softaware.Cqs
{
    public static class SoftawareCqsFluentValidationDecoratorBuilderExtensions
    {
        public static SoftawareCqsDecoratorBuilder AddFluentValidationDecorator(
            this SoftawareCqsDecoratorBuilder decoratorBuilder,
            Action<SoftawareCqsTypesBuilder> validatorTypesBuilderAction)
        {
            var typesBuilder = new SoftawareCqsTypesBuilder();
            validatorTypesBuilderAction.Invoke(typesBuilder);

            // Register all fluent validators which are available in the specified assemblies.
            decoratorBuilder.Container.Register(typeof(IValidator<>), typesBuilder.RegisteredAssemblies);

            // Register "empty" validator if no other matching validator exists.
            decoratorBuilder.Container.RegisterConditional(typeof(IValidator<>), typeof(NullValidator<>), c => !c.Handled);

            decoratorBuilder.Container.RegisterDecorator(typeof(ICommandHandler<>), typeof(FluentValidationCommandHandlerDecorator<>));
            decoratorBuilder.Container.RegisterDecorator(typeof(IQueryHandler<,>), typeof(FluentValidationQueryHandlerDecorator<,>));

            return decoratorBuilder;
        }
    }
}
