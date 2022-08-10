namespace softaware.Cqs.DependencyInjection;

internal static class TypeHelper
{
    public static readonly Type RequestHandlerGenericTypeDefinition = typeof(IRequestHandler<,>);

    public static Type GetConcreteDecoratorInterface(this Type type)
    {
        var concreteInterface = type.GetInterfaces().SingleOrDefault(
            type => type.IsGenericType && type.GetGenericTypeDefinition() == RequestHandlerGenericTypeDefinition);

        if (concreteInterface == null)
        {
            throw new ArgumentException($"Unable to find concerete interface for decorator type '{type}'.");
        }

        return concreteInterface;
    }

    public static bool IsDecorator(this Type type)
    {
        var decoratorInterface = GetConcreteDecoratorInterface(type);
        var isDecorator = type.GetConstructors()
            .SelectMany(c => c.GetParameters())
            .Any(p => p.ParameterType == decoratorInterface);

        return isDecorator;
    }
}
