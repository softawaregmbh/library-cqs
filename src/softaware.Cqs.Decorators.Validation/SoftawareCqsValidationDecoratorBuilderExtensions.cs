using softaware.Cqs.Decorators.Validation;

namespace softaware.Cqs
{
    public static class SoftawareCqsValidationDecoratorBuilderExtensions
    {
        public static SoftawareCqsDecoratorBuilder AddValidationDecorator(this SoftawareCqsDecoratorBuilder decoratorBuilder)
        {
            decoratorBuilder.Container.RegisterInstance<IValidator>(new DataAnnotationsValidator());

            decoratorBuilder.AddCommandHandlerDecorator(typeof(ValidationCommandHandlerDecorator<>));
            decoratorBuilder.AddQueryHandlerDecorator(typeof(ValidationQueryHandlerDecorator<,>));

            return decoratorBuilder;
        }
    }
}
