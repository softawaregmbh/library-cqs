namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Provides methods for configuring decorators for the softaware CQS infrastructure with source generation.
/// </summary>
public class SoftawareCqsDecoratorBuilder
{
    internal HashSet<Type> EnabledDecorators { get; } = [];

    /// <summary>
    /// Adds a request handler decorator.
    /// </summary>
    /// <remarks>
    /// The source generator reads the <c>typeof()</c> argument from the syntax tree at compile time
    /// and generates explicit decorator chains for each handler.
    /// At runtime, the type is recorded so that conditional registrations (inside <c>if</c> blocks)
    /// can be evaluated correctly.
    /// </remarks>
    /// <param name="decoratorType">
    /// The type of the decorator. The decorator must implement
    /// <c>IRequestHandler&lt;TRequest, TResult&gt;</c>.
    /// </param>
    /// <returns>The decorator builder for chaining.</returns>
    public SoftawareCqsDecoratorBuilder AddRequestHandlerDecorator(Type decoratorType)
    {
        if (decoratorType is null)
        {
            throw new ArgumentNullException(nameof(decoratorType));
        }

        var normalizedDecoratorType = decoratorType.IsConstructedGenericType
            ? decoratorType.GetGenericTypeDefinition()
            : decoratorType;

        this.EnabledDecorators.Add(normalizedDecoratorType);
        return this;
    }
}
