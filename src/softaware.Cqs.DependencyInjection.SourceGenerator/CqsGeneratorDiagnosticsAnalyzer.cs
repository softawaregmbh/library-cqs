using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace softaware.Cqs.DependencyInjection.SourceGenerator;

/// <summary>
/// Declares the diagnostics reported by <see cref="CqsSourceGenerator"/> so that the Roslyn
/// release-tracking analyzer (RS2000/RS2002) can validate <c>AnalyzerReleases.Unshipped.md</c>.
/// All diagnostics are actually emitted by the source generator; this class exists solely to
/// satisfy the release-tracking infrastructure.
/// </summary>
/// <remarks>
/// Descriptors must be defined as static fields directly in this class (not via cross-type
/// references). The restrictions is required by the release-tracking analyzer
/// in Microsoft.CodeAnalysis.Analyzers; see https://github.com/dotnet/roslyn-analyzers/issues/5828.
/// </remarks>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
internal sealed class CqsGeneratorDiagnosticsAnalyzer : DiagnosticAnalyzer
{
    internal static readonly DiagnosticDescriptor MissingHandler = new(
        "CQ0002",
        "Missing request handler",
        "Request type '{0}' has no registered IRequestHandler<{0}, {1}>",
        "Usage",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "Every request type (ICommand/IQuery) must have a corresponding handler registered via IncludeTypesFrom.");

    internal static readonly DiagnosticDescriptor UnsupportedAssemblyOverload = new(
        "CQ0003",
        "Unsupported IncludeTypesFrom overload",
        "IncludeTypesFrom(Assembly...) is not supported by the source generator. Use IncludeTypesFrom(typeof(MarkerType)) instead.",
        "Usage",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "The source generator can only resolve types known at compile time. Use typeof() expressions instead of Assembly references.");

    internal static readonly DiagnosticDescriptor ConvenienceMethodDetected = new(
        "CQ0004",
        "Unsupported convenience method",
        "Convenience method '{0}' is not supported by the source generator. Use AddRequestHandlerDecorator(typeof(...)) directly.",
        "Usage",
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "The source generator cannot trace through convenience extension methods. Register decorators directly via AddRequestHandlerDecorator(typeof(...)).");

    internal static readonly DiagnosticDescriptor NoConfigurationFound = new(
        "CQ0005",
        "No CQS configuration found",
        "No AddSoftawareCqs call found in the compilation. The source generator has nothing to generate.",
        "Usage",
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "The source generator could not find any AddSoftawareCqs() call. Ensure you call services.AddSoftawareCqs(b => b.IncludeTypesFrom(typeof(YourMarkerType))). Suppress this warning with <NoWarn>CQ0005</NoWarn>.");

    internal static readonly DiagnosticDescriptor CoreTypesNotFound = new(
        "CQ0006",
        "Core CQS types not resolved",
        "Could not resolve softaware.Cqs core types (IRequestHandler, IRequest, IRequestProcessor). Ensure the softaware.CQS NuGet package is referenced.",
        "Usage",
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "The source generator requires the softaware.CQS core package to resolve handler and request types.");

    internal static readonly DiagnosticDescriptor GenerationSucceeded = new(
        "CQ0007",
        "CQS source generation succeeded",
        "softaware.Cqs source generator: Registered {0} handler(s) with {1} decorator(s). IRequestProcessor -> GeneratedRequestProcessor.",
        "Usage",
        DiagnosticSeverity.Info,
        isEnabledByDefault: true,
        description: "The source generator successfully generated handler registrations (`CqsServiceRegistration.g.cs`) and the static request processor (`GeneratedRequestProcessor.g.cs`).");

    internal static readonly DiagnosticDescriptor TypeofExpressionRequired = new(
        "CQ0008",
        "Argument must be a typeof() expression",
        "Argument to '{0}' must be a typeof() expression (e.g. typeof(MyType)). The source generator reads types from the syntax tree at compile time and cannot evaluate variables or method calls.",
        "Usage",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "The source generator resolves types at compile time by reading typeof() expressions directly from the syntax tree. Variables, method calls, or other expressions cannot be evaluated. Use a marker type/interface and typeof(Marker) instead.");

    internal static readonly DiagnosticDescriptor OpenGenericRequestNotSupported = new(
        "CQ0009",
        "Open generic request type not supported",
        "Handler '{0}' uses open generic request type '{1}'. Open generic requests (e.g. MyRequest<TEntity>) are not supported by the source generator.",
        "Usage",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "The source generator cannot register handlers for open generic request types because it generates explicit registrations for each concrete request type. Refactor to use a closed generic type or a non-generic base type.");

    internal static readonly DiagnosticDescriptor ConditionalDecoratorRegistration = new(
        "CQ0010",
        "Conditional decorator registration detected",
        "AddRequestHandlerDecorator inside a conditional block will use a runtime registry check. Ensure the AddDecorators lambda is executed at runtime.",
        "Usage",
        DiagnosticSeverity.Info,
        isEnabledByDefault: true,
        description: "The source generator detected an AddRequestHandlerDecorator call inside an if/switch block. The generated code will check a runtime registry to determine if the decorator should be applied. The AddDecorators lambda must be executed at runtime for conditional decorators to work correctly.");

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        ImmutableArray.Create(
            MissingHandler,
            UnsupportedAssemblyOverload,
            ConvenienceMethodDetected,
            NoConfigurationFound,
            CoreTypesNotFound,
            GenerationSucceeded,
            TypeofExpressionRequired,
            OpenGenericRequestNotSupported,
            ConditionalDecoratorRegistration);

    public override void Initialize(AnalysisContext context)
    {
        // This analyzer exists solely to declare the diagnostics emitted by CqsSourceGenerator
        // so that the Roslyn release-tracking infrastructure (RS2000/RS2002) can validate them
        // against AnalyzerReleases.Unshipped.md / AnalyzerReleases.Shipped.md.
        // No analysis actions are registered here; all diagnostics are reported by the generator.
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
    }
}
