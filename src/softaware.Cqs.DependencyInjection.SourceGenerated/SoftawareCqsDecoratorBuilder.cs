namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Provides methods for configuring decorators for the softaware CQS infrastructure with source generation.
/// </summary>
public class SoftawareCqsDecoratorBuilder
{
    /// <summary>
    /// Adds a request handler decorator.
    /// </summary>
    /// <remarks>
    /// At runtime this method is a no-op. The source generator reads the <c>typeof()</c> argument
    /// from the syntax tree at compile time and generates explicit decorator chains for each handler.
    /// </remarks>
    /// <param name="decoratorType">
    /// The type of the decorator. The decorator must implement
    /// <c>IRequestHandler&lt;TRequest, TResult&gt;</c>.
    /// </param>
    /// <returns>The decorator builder for chaining.</returns>
    public SoftawareCqsDecoratorBuilder AddRequestHandlerDecorator(Type decoratorType)
    {
        // No-op at runtime. The source generator reads this call syntactically at compile time.
        return this;
    }
}
