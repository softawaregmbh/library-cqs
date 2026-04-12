using System.Collections.Generic;
using Microsoft.CodeAnalysis;

namespace softaware.Cqs.DependencyInjection.SourceGenerator;

/// <summary>
/// Parsed CQS configuration extracted from the syntax tree.
/// </summary>
internal sealed class CqsConfiguration
{
    /// <summary>
    /// Marker types from IncludeTypesFrom(typeof(...)). Used to identify assemblies containing handlers.
    /// </summary>
    public List<INamedTypeSymbol> MarkerTypes { get; } = new();

    /// <summary>
    /// Decorator types from AddRequestHandlerDecorator(typeof(...)), in registration order.
    /// First registered = closest to handler, last registered = outermost.
    /// </summary>
    public List<INamedTypeSymbol> DecoratorTypes { get; } = new();

    /// <summary>
    /// Location of the AddSoftawareCqs invocation (for diagnostics).
    /// </summary>
    public Location? InvocationLocation { get; set; }

    /// <summary>
    /// Diagnostics collected during syntax extraction that must be reported in the source output phase.
    /// </summary>
    public List<PendingDiagnostic> PendingDiagnostics { get; } = new();
}

/// <summary>
/// A diagnostic that was detected during syntax extraction but must be reported later.
/// </summary>
internal sealed class PendingDiagnostic
{
    public DiagnosticDescriptor Descriptor { get; set; } = null!;
    public Location Location { get; set; } = Location.None;
    public object[] MessageArgs { get; set; } = new object[0];
}

/// <summary>
/// A discovered handler with its request type, result type, and applicable decorator chain.
/// </summary>
internal sealed class HandlerInfo
{
    public INamedTypeSymbol HandlerType { get; set; } = null!;
    public INamedTypeSymbol RequestType { get; set; } = null!;
    public INamedTypeSymbol ResultType { get; set; } = null!;

    /// <summary>
    /// Decorators applicable to this handler, in registration order (first = closest to handler).
    /// </summary>
    public List<INamedTypeSymbol> ApplicableDecorators { get; } = new();
}
