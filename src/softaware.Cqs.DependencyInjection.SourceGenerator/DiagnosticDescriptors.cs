using Microsoft.CodeAnalysis;

namespace softaware.Cqs.DependencyInjection.SourceGenerator;

/// <summary>
/// Diagnostic descriptors for the CQS source generator.
/// </summary>
/// <remarks>
/// Descriptor instances are defined in <see cref="CqsGeneratorDiagnosticsAnalyzer"/> so that
/// the Roslyn release-tracking analyzer can discover them via <c>SupportedDiagnostics</c>.
/// This class provides named aliases for use throughout <see cref="CqsSourceGenerator"/>.
/// </remarks>
internal static class DiagnosticDescriptors
{
    public static readonly DiagnosticDescriptor MissingHandler = CqsGeneratorDiagnosticsAnalyzer.MissingHandler;
    public static readonly DiagnosticDescriptor UnsupportedAssemblyOverload = CqsGeneratorDiagnosticsAnalyzer.UnsupportedAssemblyOverload;
    public static readonly DiagnosticDescriptor ConvenienceMethodDetected = CqsGeneratorDiagnosticsAnalyzer.ConvenienceMethodDetected;
    public static readonly DiagnosticDescriptor NoConfigurationFound = CqsGeneratorDiagnosticsAnalyzer.NoConfigurationFound;
    public static readonly DiagnosticDescriptor CoreTypesNotFound = CqsGeneratorDiagnosticsAnalyzer.CoreTypesNotFound;
    public static readonly DiagnosticDescriptor GenerationSucceeded = CqsGeneratorDiagnosticsAnalyzer.GenerationSucceeded;
    public static readonly DiagnosticDescriptor TypeofExpressionRequired = CqsGeneratorDiagnosticsAnalyzer.TypeofExpressionRequired;
    public static readonly DiagnosticDescriptor OpenGenericRequestNotSupported = CqsGeneratorDiagnosticsAnalyzer.OpenGenericRequestNotSupported;
    public static readonly DiagnosticDescriptor ConditionalDecoratorRegistration = CqsGeneratorDiagnosticsAnalyzer.ConditionalDecoratorRegistration;
}
