using Microsoft.CodeAnalysis;

namespace softaware.Cqs.DependencyInjection.SourceGenerator;

/// <summary>
/// Diagnostic descriptors for the CQS source generator.
/// </summary>
internal static class DiagnosticDescriptors
{
    public static readonly DiagnosticDescriptor MissingHandler = new(
        id: "CQ0002",
        title: "Missing request handler",
        messageFormat: "Request type '{0}' has no registered IRequestHandler<{0}, {1}>",
        category: "softaware.Cqs",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "Every request type (ICommand/IQuery) must have a corresponding handler registered via IncludeTypesFrom.");

    public static readonly DiagnosticDescriptor UnsupportedAssemblyOverload = new(
        id: "CQ0003",
        title: "Unsupported IncludeTypesFrom overload",
        messageFormat: "IncludeTypesFrom(Assembly...) is not supported by the source generator. Use IncludeTypesFrom(typeof(MarkerType)) instead",
        category: "softaware.Cqs",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "The source generator can only resolve types known at compile time. Use typeof() expressions instead of Assembly references.");

    public static readonly DiagnosticDescriptor ConvenienceMethodDetected = new(
        id: "CQ0004",
        title: "Unsupported convenience method",
        messageFormat: "Convenience method '{0}' is not supported by the source generator. Use AddRequestHandlerDecorator(typeof(...)) directly",
        category: "softaware.Cqs",
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "The source generator cannot trace through convenience extension methods. Register decorators directly via AddRequestHandlerDecorator(typeof(...)).");

    public static readonly DiagnosticDescriptor NoConfigurationFound = new(
        id: "CQ0005",
        title: "No CQS configuration found",
        messageFormat: "No AddSoftawareCqs call found in the compilation. The source generator has nothing to generate",
        category: "softaware.Cqs",
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "The source generator could not find any AddSoftawareCqs() call. Ensure you call services.AddSoftawareCqs(b => b.IncludeTypesFrom(typeof(YourMarkerType))). Suppress this warning with <NoWarn>CQ0005</NoWarn>.");

    public static readonly DiagnosticDescriptor CoreTypesNotFound = new(
        id: "CQ0006",
        title: "Core CQS types not resolved",
        messageFormat: "Could not resolve softaware.Cqs core types (IRequestHandler, IRequest, IRequestProcessor). Ensure the softaware.CQS NuGet package is referenced",
        category: "softaware.Cqs",
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "The source generator requires the softaware.CQS core package to resolve handler and request types.");

    public static readonly DiagnosticDescriptor GenerationSucceeded = new(
        id: "CQ0007",
        title: "CQS source generation succeeded",
        messageFormat: "softaware.Cqs source generator: Registered {0} handler(s) with {1} decorator(s). IRequestProcessor → GeneratedRequestProcessor",
        category: "softaware.Cqs",
        defaultSeverity: DiagnosticSeverity.Info,
        isEnabledByDefault: true,
        description: "The source generator successfully generated handler registrations (`CqsServiceRegistration.g.cs`) and the static request processor (`GeneratedRequestProcessor.g.cs`).");

    public static readonly DiagnosticDescriptor TypeofExpressionRequired = new(
        id: "CQ0008",
        title: "Argument must be a typeof() expression",
        messageFormat: "Argument to '{0}' must be a typeof() expression (e.g. typeof(MyType)). The source generator reads types from the syntax tree at compile time and cannot evaluate variables or method calls",
        category: "softaware.Cqs",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "The source generator resolves types at compile time by reading typeof() expressions directly from the syntax tree. Variables, method calls, or other expressions cannot be evaluated. Use a marker type/interface and typeof(Marker) instead.");

    public static readonly DiagnosticDescriptor OpenGenericRequestNotSupported = new(
        id: "CQ0009",
        title: "Open generic request type not supported",
        messageFormat: "Handler '{0}' uses open generic request type '{1}'. Open generic requests (e.g. MyRequest<TEntity>) are not supported by the source generator",
        category: "softaware.Cqs",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "The source generator cannot register handlers for open generic request types because it generates explicit registrations for each concrete request type. Refactor to use a closed generic type or a non-generic base type.");

    public static readonly DiagnosticDescriptor ConditionalDecoratorRegistration = new(
        id: "CQ0010",
        title: "Conditional decorator registration detected",
        messageFormat: "AddRequestHandlerDecorator inside a conditional block will use a runtime registry check. Ensure the AddDecorators lambda is executed at runtime",
        category: "softaware.Cqs",
        defaultSeverity: DiagnosticSeverity.Info,
        isEnabledByDefault: true,
        description: "The source generator detected an AddRequestHandlerDecorator call inside an if/switch block. The generated code will check a runtime registry to determine if the decorator should be applied. The AddDecorators lambda must be executed at runtime for conditional decorators to work correctly.");
}
