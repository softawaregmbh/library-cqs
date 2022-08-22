using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace softaware.Cqs.Analyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class IRequestShouldNotBeImplementedDirectlyAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "CQ0001";

        // You can change these strings in the Resources.resx file. If you do not want your analyzer to be localize-able, you can use regular strings for Title and MessageFormat.
        // See https://github.com/dotnet/roslyn/blob/main/docs/analyzers/Localizing%20Analyzers.md for more on localization
        private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Resources.AnalyzerTitle), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Resources.AnalyzerMessageFormat), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString Description = new LocalizableResourceString(nameof(Resources.AnalyzerDescription), Resources.ResourceManager, typeof(Resources));
        private const string Category = "Usage";

#pragma warning disable IDE0090 // Use 'new(...)' https://github.com/dotnet/roslyn-analyzers/issues/5828
        private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Error, isEnabledByDefault: true, description: Description);
#pragma warning restore IDE0090 // Use 'new(...)'

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();

            // https://github.com/dotnet/roslyn-sdk/blob/main/samples/CSharp/Analyzers/Analyzers.Implementation/StatefulAnalyzers/CompilationStartedAnalyzer.cs
            context.RegisterCompilationStartAction(compilationContext =>
            {
                // Only check compilations where where ICommand (and thus softaware.Cqs) is available
                INamedTypeSymbol iCommandType = compilationContext.Compilation.GetTypeByMetadataName("softaware.Cqs.ICommand");
                if (iCommandType == null)
                {
                    return;
                }

                var iRequestType = compilationContext.Compilation.GetTypeByMetadataName("softaware.Cqs.IRequest`1");
                compilationContext.RegisterSymbolAction(symbolContext => AnalyzeSymbol(symbolContext, iRequestType), SymbolKind.NamedType);
            });
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("MicrosoftCodeAnalysisCorrectness", "RS1030:Do not invoke Compilation.GetSemanticModel() method within a diagnostic analyzer", Justification = "Not sure how to implement this otherwise.")]
        private static void AnalyzeSymbol(SymbolAnalysisContext context, params INamedTypeSymbol[] forbiddenInterfaceTypes)
        {
            var namedType = (INamedTypeSymbol)context.Symbol;

            foreach (var implementedInterface in namedType.Interfaces)
            {
                var interfaceType = implementedInterface.IsGenericType
                    ? implementedInterface.ConstructedFrom // get IRequest<T> type definition from IRequest<ConcreteType>
                    : implementedInterface;

                foreach (var forbiddenInterface in forbiddenInterfaceTypes)
                {
                    if (interfaceType.Equals(forbiddenInterface, SymbolEqualityComparer.Default))
                    {
                        // try to find the location of the actual interface implementation
                        var interfaceImplementationLocations =
                            from syntax in namedType.DeclaringSyntaxReferences
                            let declaration = syntax.GetSyntax() as ClassDeclarationSyntax
                            where declaration != null
                            let model = context.Compilation.GetSemanticModel(syntax.SyntaxTree)
                            from type in declaration.BaseList.Types
                            let symbol = model.GetSymbolInfo(type.Type)
                            where symbol.Symbol != null && symbol.Symbol.Equals(implementedInterface, SymbolEqualityComparer.Default)
                            select type.GetLocation();

                        // use the class declaration location as backup
                        var location = interfaceImplementationLocations.FirstOrDefault() ?? namedType.Locations.First();

                        var diagnostic = Diagnostic.Create(Rule, location, namedType.Name, forbiddenInterface.Name);
                        context.ReportDiagnostic(diagnostic);
                    }
                }
            }
        }
    }
}
