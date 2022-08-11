namespace softaware.Cqs.DependencyInjection;

internal static class RequestHandlerTypeHelper
{
    public static readonly Type GenericRequestHandlerTypeDefinition = typeof(IRequestHandler<,>);

    /// <summary>
    /// Tries to find the <see cref="IRequestHandler{TRequest, TResult}"/> interface with
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
    /// <param name="type">The type to search for the <see cref="IRequestHandler{TRequest, TResult}"/> interface.</param>
    /// <returns>The implemented <see cref="IRequestHandler{TRequest, TResult}"/> interface type
    /// with type arguments, or <c>null</c> if none is found.</returns>
    public static Type? GetImplementedRequestHandlerInterfaceType(this Type type)
    {
        var requestHandlerInterfaceType = type.GetInterfaces().SingleOrDefault(
            type => type.IsGenericType && type.GetGenericTypeDefinition() == GenericRequestHandlerTypeDefinition);

        return requestHandlerInterfaceType;
    }

    /// <summary>
    /// Checks if a type is a decorator. For our purpose, we define a decorator as a type implementing <see cref="IRequestHandler{TRequest, TResult}"/>
    /// that has a constructor parameter of the same interface type.
    /// </summary>
    /// <param name="type">The type to check.</param>
    public static bool IsDecorator(this Type type)
    {
        if (type.IsInterface)
        {
            return false;
        }

        var requestHandlerInterfaceType = GetImplementedRequestHandlerInterfaceType(type);
        if (requestHandlerInterfaceType == null)
        {
            return false;
        }

        var hasConstructorParameterOfSameInterfaceType =
            type.GetConstructors()
                .SelectMany(c => c.GetParameters())
                .Any(p => p.ParameterType == requestHandlerInterfaceType);

        return hasConstructorParameterOfSameInterfaceType;
    }
}
