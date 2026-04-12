using System.ComponentModel;
using System.Reflection;
using System.Runtime.CompilerServices;
using softaware.Cqs;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Provides extension methods for <see cref="IServiceCollection"/> to configure the softaware CQS infrastructure
/// using compile-time source generation.
/// </summary>
public static class SoftawareCqsExtensions
{
    /// <summary>
    /// Adds the softaware CQS infrastructure using compile-time generated registrations.
    /// </summary>
    /// <remarks>
    /// This method locates the source-generated <c>CqsServiceRegistration</c> class in the calling assembly
    /// and invokes its <c>RegisterAll</c> method to register all handlers and decorators.
    /// The <paramref name="softawareCqsTypesBuilderAction"/> is read syntactically by the source generator
    /// at compile time; at runtime it is not executed.
    /// </remarks>
    /// <param name="services">The service collection.</param>
    /// <param name="softawareCqsTypesBuilderAction">
    /// The types builder for specifying assemblies via <c>IncludeTypesFrom(typeof(...))</c>.
    /// Only <c>typeof()</c> expressions are supported; <c>Assembly.GetExecutingAssembly()</c> is not.
    /// </param>
    /// <returns>The softaware CQS builder.</returns>
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static SoftawareCqsBuilder AddSoftawareCqs(
        this IServiceCollection services,
        Action<SoftawareCqsTypesBuilder> softawareCqsTypesBuilderAction)
    {
        var callingAssembly = Assembly.GetCallingAssembly();
        var registrationType = callingAssembly.GetType("softaware.Cqs.Generated.CqsServiceRegistration");

        if (registrationType == null)
        {
            throw new InvalidOperationException(
                $"Source-generated CQS registration class not found in assembly '{callingAssembly.GetName().Name}'. " +
                "Ensure the softaware.Cqs.DependencyInjection.SourceGenerated NuGet package is referenced " +
                "(which includes the source generator), and that the project has been rebuilt.");
        }

        var method = registrationType.GetMethod("RegisterAll", BindingFlags.Public | BindingFlags.Static);
        method!.Invoke(null, new object[] { services });

        return new SoftawareCqsBuilder(services);
    }
}
