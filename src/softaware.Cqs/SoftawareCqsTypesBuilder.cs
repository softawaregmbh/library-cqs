using System.Reflection;

namespace softaware.Cqs;

/// <summary>
/// Provides a builder to configure registered assemblies.
/// </summary>
public class SoftawareCqsTypesBuilder
{
    private readonly HashSet<Assembly> registeredAssemblies = new();

    /// <summary>
    /// Gets the already registered assemblies.
    /// </summary>
    public IReadOnlyCollection<Assembly> RegisteredAssemblies => this.registeredAssemblies;

    /// <summary>
    /// Registers the assemblies of the provided <paramref name="markerTypes"/>.
    /// </summary>
    public SoftawareCqsTypesBuilder IncludeTypesFrom(params Type[] markerTypes)
    {
        this.IncludeTypesFrom(markerTypes.Select(t => t.Assembly).ToArray());
        return this;
    }

    /// <summary>
    /// Registers the provided <paramref name="assemblies"/> for later CQS configuration.
    /// </summary>
    public SoftawareCqsTypesBuilder IncludeTypesFrom(params Assembly[] assemblies)
    {
        foreach (var assembly in assemblies)
        {
            this.registeredAssemblies.Add(assembly);
        }

        return this;
    }
}
