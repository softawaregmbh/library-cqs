namespace softaware.Cqs.DependencyInjection;

internal static class RequestHandlerTypeHelper
{
    public static readonly Type GenericRequestHandlerTypeDefinition = typeof(IRequestHandler<,>);

    /// <summary>
    /// Tries to find the <see cref="IRequestHandler{TRequest, TResult}"/> interfaces with
    /// type arguments the provided <paramref name="type"/> implements.
    /// The type arguments can either be concrete types, type arguments of the provided
    /// <paramref name="type"/>, or a combination - which is currently not supported):
    /// <code>
    /// class Decorator : IRequestHandler&lt;ICommand, NoResult&gt;
    ///                   ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
    /// class Decorator&lt;TRequest, TResult&gt; : IRequestHandler&lt;TRequest, TResult&gt;
    ///                                      ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
    /// class Decorator&lt;TRequest, NoResult&gt; : IRequestHandler&lt;TRequest, NoResult&gt;
    ///                                       ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
    /// </code>
    /// </summary>
    /// <param name="type">The type to search for the <see cref="IRequestHandler{TRequest, TResult}"/> interfaces.</param>
    /// <returns>The implemented <see cref="IRequestHandler{TRequest, TResult}"/> interface types
    /// with type arguments. Can be empty if none is found.</returns>
    public static IEnumerable<Type> GetImplementedRequestHandlerInterfaceTypes(this Type type)
    {
        var requestHandlerInterfaceTypes = type.GetInterfaces()
            .Where(type => type.IsGenericType && type.GetGenericTypeDefinition() == GenericRequestHandlerTypeDefinition);

        return requestHandlerInterfaceTypes;
    }

    /// <summary>
    /// Checks if a type is a decorator. For our purpose, we define a decorator as a type implementing <see cref="IRequestHandler{TRequest, TResult}"/>
    /// that has a constructor parameter of the same interface type.
    /// </summary>
    /// <remarks>
    /// If the decorator implements multiple <see cref="IRequestHandler{TRequest, TResult}"/> interfaces,
    /// it is still a decorator but we don't support this case and treat it as invalid decorator configuration.
    /// This distinction is needed so that we don't register this invalid decorator configuration as regular <see cref="IRequestHandler{TRequest, TResult}"/> and therfore avoid registering handlers for the same <see cref="IRequest{TResult}"/> multiple times.
    /// </remarks>
    /// <param name="type">The type to check.</param>
    public static (bool IsDecorator, bool IsValidDecoratorConfiguration) GetDecoratorInfo(this Type type)
    {
        if (type.IsInterface)
        {
            return (IsDecorator: false, IsValidDecoratorConfiguration: false);
        }

        var requestHandlerInterfaceTypes = GetImplementedRequestHandlerInterfaceTypes(type);
        if (!requestHandlerInterfaceTypes.Any())
        {
            return (IsDecorator: false, IsValidDecoratorConfiguration: false);
        }

        var isDecorator = requestHandlerInterfaceTypes.All(requestHandlerInterfaceType =>
        {
            var hasConstructorParameterOfSameInterfaceType =
                type.GetConstructors()
                    .SelectMany(c => c.GetParameters())
                    .Any(p => p.ParameterType == requestHandlerInterfaceType);

            return hasConstructorParameterOfSameInterfaceType;
        });

        return (isDecorator, IsValidDecoratorConfiguration: requestHandlerInterfaceTypes.Count() == 1);
    }
}
