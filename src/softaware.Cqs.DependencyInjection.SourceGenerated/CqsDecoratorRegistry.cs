namespace softaware.Cqs.DependencyInjection;

/// <summary>
/// Tracks which decorator types were registered at runtime via <c>AddDecorators</c>.
/// Used by the source-generated factory lambdas to evaluate conditional decorator registrations
/// (i.e. decorators inside <c>if</c>/<c>switch</c> blocks).
/// </summary>
public sealed class CqsDecoratorRegistry
{
    private readonly HashSet<Type> enabledDecorators;

    /// <summary>
    /// Creates an empty registry (no decorators enabled).
    /// </summary>
    public CqsDecoratorRegistry() => this.enabledDecorators = [];

    /// <summary>
    /// Creates a registry with the specified decorator types enabled.
    /// </summary>
    /// <param name="enabledDecorators">The set of open generic decorator types that were registered at runtime.</param>
    public CqsDecoratorRegistry(HashSet<Type> enabledDecorators) => this.enabledDecorators = enabledDecorators ?? throw new ArgumentNullException(nameof(enabledDecorators));

    /// <summary>
    /// Returns <c>true</c> if the given open generic decorator type was registered at runtime.
    /// </summary>
    /// <param name="openGenericDecoratorType">The open generic decorator type (e.g. <c>typeof(MyDecorator&lt;,&gt;)</c>).</param>
    public bool IsEnabled(Type openGenericDecoratorType)
    {
        return this.enabledDecorators.Contains(openGenericDecoratorType);
    }
}
