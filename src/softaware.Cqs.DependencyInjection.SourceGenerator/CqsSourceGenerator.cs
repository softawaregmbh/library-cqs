using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace softaware.Cqs.DependencyInjection.SourceGenerator;

[Generator]
public class CqsSourceGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var debugEnabled = context.AnalyzerConfigOptionsProvider
            .Select(static (options, _) =>
                options.GlobalOptions.TryGetValue("build_property.CqsDebugSourceGenerator", out var value)
                && string.Equals(value, "true", StringComparison.OrdinalIgnoreCase));

        // Find all InvocationExpression nodes that could be AddSoftawareCqs calls
        var cqsInvocations = context.SyntaxProvider.CreateSyntaxProvider(
            predicate: static (node, _) => IsAddSoftawareCqsCandidate(node),
            transform: static (ctx, ct) => ExtractConfiguration(ctx, ct))
            .Where(static c => c is not null);

        // Find convenience method calls (for warning diagnostic)
        var convenienceMethodCalls = context.SyntaxProvider.CreateSyntaxProvider(
            predicate: static (node, _) => IsConvenienceMethodCandidate(node),
            transform: static (ctx, ct) => ExtractConvenienceMethodInfo(ctx))
            .Where(static info => info is not null);

        // Combine configurations with compilation
        var compilationAndConfigs = context.CompilationProvider
            .Combine(cqsInvocations.Collect())
            .Combine(convenienceMethodCalls.Collect());

        context.RegisterSourceOutput(compilationAndConfigs.Combine(debugEnabled), static (spc, source) =>
        {
            var (data, shouldDebug) = source;
            if (shouldDebug && !Debugger.IsAttached)
            {
                Debugger.Launch();
            }
            var ((compilation, configurations), convenienceMethods) = data;
            Execute(compilation, configurations, convenienceMethods, spc);
        });
    }

    private static bool IsAddSoftawareCqsCandidate(SyntaxNode node)
    {
        if (node is not InvocationExpressionSyntax invocation)
        {
            return false;
        }

        var name = GetMethodName(invocation);
        return name == "AddSoftawareCqs";
    }

    private static bool IsConvenienceMethodCandidate(SyntaxNode node)
    {
        if (node is not InvocationExpressionSyntax invocation)
        {
            return false;
        }

        var name = GetMethodName(invocation);
        return name is "AddTransactionCommandHandlerDecorator"
            or "AddTransactionQueryHandlerDecorator"
            or "AddDataAnnotationsValidationDecorators"
            or "AddFluentValidationDecorators"
            or "AddUsageAwareDecorators"
            or "AddApplicationInsightsDependencyTelemetryDecorator";
    }

    private static string? GetMethodName(InvocationExpressionSyntax invocation)
    {
        return invocation.Expression switch
        {
            MemberAccessExpressionSyntax memberAccess => memberAccess.Name.Identifier.Text,
            IdentifierNameSyntax identifier => identifier.Identifier.Text,
            _ => null
        };
    }

    private static bool IsInsideConditional(SyntaxNode node, SyntaxNode boundary)
    {
        var current = node.Parent;
        while (current != null && current != boundary)
        {
            if (current is IfStatementSyntax or SwitchStatementSyntax or SwitchExpressionSyntax or ConditionalExpressionSyntax)
            {
                return true;
            }

            current = current.Parent;
        }
        return false;
    }

    private static CqsConfiguration? ExtractConfiguration(GeneratorSyntaxContext context, CancellationToken ct)
    {
        var invocation = (InvocationExpressionSyntax)context.Node;
        var config = new CqsConfiguration
        {
            InvocationLocation = invocation.GetLocation()
        };

        // Extract IncludeTypesFrom(typeof(...)) from the lambda argument
        if (invocation.ArgumentList.Arguments.Count > 0)
        {
            var lambdaArg = invocation.ArgumentList.Arguments[0].Expression;
            ExtractMarkerTypes(lambdaArg, context.SemanticModel, config, ct);
        }

        // Walk up the syntax tree to find chained .AddDecorators(...) calls
        ExtractDecoratorTypes(invocation, context.SemanticModel, config, ct);

        if (config.MarkerTypes.Count == 0 && config.PendingDiagnostics.Count == 0)
        {
            return null;
        }

        return config;
    }

    private static void ExtractMarkerTypes(
        ExpressionSyntax lambdaExpression,
        SemanticModel semanticModel,
        CqsConfiguration config,
        CancellationToken ct)
    {
        // Find all typeof expressions within IncludeTypesFrom calls
        foreach (var invocation in lambdaExpression.DescendantNodesAndSelf().OfType<InvocationExpressionSyntax>())
        {
            var name = GetMethodName(invocation);

            if (name == "IncludeTypesFrom")
            {
                foreach (var arg in invocation.ArgumentList.Arguments)
                {
                    if (arg.Expression is TypeOfExpressionSyntax typeOfExpr)
                    {
                        var typeInfo = semanticModel.GetTypeInfo(typeOfExpr.Type, ct);
                        if (typeInfo.Type is INamedTypeSymbol namedType)
                        {
                            config.MarkerTypes.Add(namedType);
                        }
                    }
                    else if (arg.Expression is MemberAccessExpressionSyntax memberAccess
                        && memberAccess.Name.Identifier.Text == "Assembly"
                        && memberAccess.Expression is TypeOfExpressionSyntax)
                    {
                        config.PendingDiagnostics.Add(new PendingDiagnostic
                        {
                            Descriptor = DiagnosticDescriptors.UnsupportedAssemblyOverload,
                            Location = arg.GetLocation(),
                            MessageArgs = []
                        });
                    }
                    else
                    {
                        config.PendingDiagnostics.Add(new PendingDiagnostic
                        {
                            Descriptor = DiagnosticDescriptors.TypeofExpressionRequired,
                            Location = arg.GetLocation(),
                            MessageArgs = ["IncludeTypesFrom"]
                        });
                    }
                }
            }
        }
    }

    private static void ExtractDecoratorTypes(
        InvocationExpressionSyntax addSoftawareCqsInvocation,
        SemanticModel semanticModel,
        CqsConfiguration config,
        CancellationToken ct)
    {
        // Walk up: the AddSoftawareCqs invocation might be the expression of a MemberAccess for .AddDecorators(...)
        // Pattern: services.AddSoftawareCqs(...).AddDecorators(...)
        var current = addSoftawareCqsInvocation.Parent;

        while (current != null)
        {
            if (current is MemberAccessExpressionSyntax memberAccess &&
                memberAccess.Name.Identifier.Text == "AddDecorators" &&
                memberAccess.Parent is InvocationExpressionSyntax addDecoratorsInvocation)
            {
                // Extract typeof(...) from the AddDecorators lambda
                if (addDecoratorsInvocation.ArgumentList.Arguments.Count > 0)
                {
                    var lambdaArg = addDecoratorsInvocation.ArgumentList.Arguments[0].Expression;
                    ExtractDecoratorTypesFromLambda(lambdaArg, semanticModel, config, ct);
                }

                // Continue walking up for more chained AddDecorators calls
                current = addDecoratorsInvocation.Parent;
                continue;
            }

            current = current.Parent;

            // Stop at statement level
            if (current is StatementSyntax or MemberDeclarationSyntax)
            {
                break;
            }
        }
    }

    private static void ExtractDecoratorTypesFromLambda(
        ExpressionSyntax lambdaExpression,
        SemanticModel semanticModel,
        CqsConfiguration config,
        CancellationToken ct)
    {
        // In fluent chains like b.AddA(typeof(A)).AddB(typeof(B)), both InvocationExpressions
        // start at the same SpanStart (the 'b' identifier). Use ArgumentList position to
        // preserve registration order (first-registered = closest to handler).
        var invocations = lambdaExpression
            .DescendantNodesAndSelf()
            .OfType<InvocationExpressionSyntax>()
            .Where(inv => GetMethodName(inv) == "AddRequestHandlerDecorator" && inv.ArgumentList.Arguments.Count > 0)
            .OrderBy(inv => inv.ArgumentList.SpanStart);

        foreach (var invocation in invocations)
        {
            var isConditional = IsInsideConditional(invocation, lambdaExpression);

            if (isConditional)
            {
                config.PendingDiagnostics.Add(new PendingDiagnostic
                {
                    Descriptor = DiagnosticDescriptors.ConditionalDecoratorRegistration,
                    Location = invocation.GetLocation(),
                    MessageArgs = []
                });
            }

            var arg = invocation.ArgumentList.Arguments[0].Expression;
            if (arg is TypeOfExpressionSyntax typeOfExpr)
            {
                var typeInfo = semanticModel.GetTypeInfo(typeOfExpr.Type, ct);
                if (typeInfo.Type is INamedTypeSymbol namedType)
                {
                    config.DecoratorTypes.Add(new DecoratorRegistration
                    {
                        Type = namedType.OriginalDefinition,
                        IsConditional = isConditional
                    });
                }
            }
            else
            {
                config.PendingDiagnostics.Add(new PendingDiagnostic
                {
                    Descriptor = DiagnosticDescriptors.TypeofExpressionRequired,
                    Location = arg.GetLocation(),
                    MessageArgs = ["AddRequestHandlerDecorator"]
                });
            }
        }
    }

    private static (string Name, Location Location)? ExtractConvenienceMethodInfo(
        GeneratorSyntaxContext context)
    {
        var invocation = (InvocationExpressionSyntax)context.Node;
        var name = GetMethodName(invocation);
        if (name == null)
        {
            return null;
        }

        return (name, invocation.GetLocation());
    }

    private static void Execute(
        Compilation compilation,
        ImmutableArray<CqsConfiguration?> configurations,
        ImmutableArray<(string Name, Location Location)?> convenienceMethods,
        SourceProductionContext context)
    {
        // Report convenience method warnings
        foreach (var method in convenienceMethods)
        {
            if (method.HasValue)
            {
                context.ReportDiagnostic(Diagnostic.Create(
                    DiagnosticDescriptors.ConvenienceMethodDetected,
                    method.Value.Location,
                    method.Value.Name));
            }
        }

        // Report pending diagnostics from all configurations (e.g. non-typeof arguments)
        var hasPendingDiagnostics = false;
        foreach (var config in configurations)
        {
            if (config?.PendingDiagnostics != null)
            {
                foreach (var diag in config.PendingDiagnostics)
                {
                    hasPendingDiagnostics = true;
                    context.ReportDiagnostic(Diagnostic.Create(diag.Descriptor, diag.Location, diag.MessageArgs));
                }
            }
        }

        var validConfigs = configurations.Where(c => c != null && c.MarkerTypes.Count > 0).ToList();
        if (validConfigs.Count == 0)
        {
            // Only report "no configuration found" if there are no pending error diagnostics
            // (otherwise the user already has a clear error message about what went wrong)
            if (!hasPendingDiagnostics)
            {
                context.ReportDiagnostic(Diagnostic.Create(
                    DiagnosticDescriptors.NoConfigurationFound,
                    Location.None));
            }
            return;
        }

        // Merge all configurations
        var mergedConfig = new CqsConfiguration();
        foreach (var config in validConfigs)
        {
            mergedConfig.MarkerTypes.AddRange(config!.MarkerTypes);
            mergedConfig.DecoratorTypes.AddRange(config!.DecoratorTypes);
            mergedConfig.InvocationLocation ??= config.InvocationLocation;
        }

        // Resolve core CQS type symbols
        var requestHandlerType = compilation.GetTypeByMetadataName("softaware.Cqs.IRequestHandler`2");
        var requestType = compilation.GetTypeByMetadataName("softaware.Cqs.IRequest`1");
        var commandType = compilation.GetTypeByMetadataName("softaware.Cqs.ICommand`1");
        var queryType = compilation.GetTypeByMetadataName("softaware.Cqs.IQuery`1");
        var requestProcessorType = compilation.GetTypeByMetadataName("softaware.Cqs.IRequestProcessor");

        if (requestHandlerType == null || requestType == null || requestProcessorType == null)
        {
            context.ReportDiagnostic(Diagnostic.Create(
                DiagnosticDescriptors.CoreTypesNotFound,
                mergedConfig.InvocationLocation ?? Location.None));
            return;
        }

        // Discover handlers in the assemblies of marker types
        var handlers = DiscoverHandlers(mergedConfig, requestHandlerType, context);

        // Check for request types without handlers
        ReportMissingHandlers(mergedConfig, handlers, requestType, context);

        // For each handler, determine which decorators apply
        foreach (var handler in handlers)
        {
            foreach (var decoratorReg in mergedConfig.DecoratorTypes)
            {
                if (DecoratorApplies(compilation, decoratorReg.Type, handler.RequestType, handler.ResultType, requestHandlerType))
                {
                    handler.ApplicableDecorators.Add(decoratorReg);
                }
            }
        }

        // Generate registration code
        var registrationSource = GenerateRegistrationCode(handlers);
        context.AddSource("CqsServiceRegistration.g.cs", registrationSource);

        // Generate static request processor
        var processorSource = GenerateRequestProcessorCode(handlers);
        context.AddSource("GeneratedRequestProcessor.g.cs", processorSource);

        // Report success diagnostic so users know the generator ran
        var totalDecorators = handlers.Sum(h => h.ApplicableDecorators.Count);
        context.ReportDiagnostic(Diagnostic.Create(
            DiagnosticDescriptors.GenerationSucceeded,
            mergedConfig.InvocationLocation ?? Location.None,
            handlers.Count,
            totalDecorators));
    }

    private static List<HandlerInfo> DiscoverHandlers(
        CqsConfiguration config,
        INamedTypeSymbol requestHandlerType,
        SourceProductionContext context)
    {
        var handlers = new List<HandlerInfo>();
        var processedAssemblies = new HashSet<string>();

        foreach (var markerType in config.MarkerTypes)
        {
            var assembly = markerType.ContainingAssembly;
            if (assembly == null || !processedAssemblies.Add(assembly.Name))
            {
                continue;
            }

            // Walk all types in this assembly
            var allTypes = GetAllTypes(assembly.GlobalNamespace);

            foreach (var type in allTypes)
            {
                if (type.IsAbstract || type.IsStatic || type.TypeKind != TypeKind.Class)
                {
                    continue;
                }

                // Find IRequestHandler<TRequest, TResult> implementations
                foreach (var iface in type.AllInterfaces)
                {
                    if (!SymbolEqualityComparer.Default.Equals(iface.OriginalDefinition, requestHandlerType))
                    {
                        continue;
                    }

                    if (iface.TypeArguments[0] is not INamedTypeSymbol requestArg || iface.TypeArguments[1] is not INamedTypeSymbol resultArg)
                    {
                        continue;
                    }

                    // Skip decorators (types that have a constructor parameter of IRequestHandler<,>)
                    if (IsDecorator(type, requestHandlerType))
                    {
                        continue;
                    }

                    // Open generic handlers (e.g. MyHandler<TEntity> : IRequestHandler<MyRequest<TEntity>, int>)
                    // cannot be registered because the generator produces explicit closed-type registrations.
                    if (type.TypeParameters.Length > 0)
                    {
                        context.ReportDiagnostic(Diagnostic.Create(
                            DiagnosticDescriptors.OpenGenericRequestNotSupported,
                            Location.None,
                            type.ToDisplayString(SymbolDisplayFormat.CSharpShortErrorMessageFormat),
                            requestArg.ToDisplayString(SymbolDisplayFormat.CSharpShortErrorMessageFormat)));
                        continue;
                    }

                    handlers.Add(new HandlerInfo
                    {
                        HandlerType = type,
                        RequestType = requestArg,
                        ResultType = resultArg
                    });
                }
            }
        }

        return handlers;
    }

    private static void ReportMissingHandlers(
        CqsConfiguration config,
        List<HandlerInfo> handlers,
        INamedTypeSymbol requestType,
        SourceProductionContext context)
    {
        var handledRequestTypes = new HashSet<INamedTypeSymbol>(SymbolEqualityComparer.Default);
        foreach (var handler in handlers)
        {
            handledRequestTypes.Add(handler.RequestType);
        }

        var processedAssemblies = new HashSet<string>();

        foreach (var markerType in config.MarkerTypes)
        {
            var assembly = markerType.ContainingAssembly;
            if (assembly == null || !processedAssemblies.Add(assembly.Name))
            {
                continue;
            }

            foreach (var type in GetAllTypes(assembly.GlobalNamespace))
            {
                if (type.IsAbstract || type.IsStatic || type.TypeKind != TypeKind.Class || type.TypeParameters.Length > 0)
                {
                    continue;
                }

                foreach (var iface in type.AllInterfaces)
                {
                    if (!SymbolEqualityComparer.Default.Equals(iface.OriginalDefinition, requestType))
                    {
                        continue;
                    }

                    if (!handledRequestTypes.Contains(type))
                    {
                        var resultArg = iface.TypeArguments[0];
                        context.ReportDiagnostic(Diagnostic.Create(
                            DiagnosticDescriptors.MissingHandler,
                            Location.None,
                            type.ToDisplayString(SymbolDisplayFormat.CSharpShortErrorMessageFormat),
                            resultArg.ToDisplayString(SymbolDisplayFormat.CSharpShortErrorMessageFormat)));
                    }

                    break;
                }
            }
        }
    }

    private static bool IsDecorator(INamedTypeSymbol type, INamedTypeSymbol requestHandlerType)
    {
        foreach (var constructor in type.Constructors)
        {
            foreach (var param in constructor.Parameters)
            {
                if (param.Type is INamedTypeSymbol paramType &&
                    paramType.IsGenericType &&
                    SymbolEqualityComparer.Default.Equals(paramType.OriginalDefinition, requestHandlerType))
                {
                    return true;
                }
            }
        }

        return false;
    }

    private static bool DecoratorApplies(
        Compilation compilation,
        INamedTypeSymbol openDecoratorType,
        INamedTypeSymbol requestType,
        INamedTypeSymbol resultType,
        INamedTypeSymbol requestHandlerType)
    {
        // The decorator is an open generic like Decorator<TRequest, TResult>
        // We need to check if the constraints on TRequest (and TResult) are satisfied
        // by the concrete requestType and resultType.

        if (!openDecoratorType.IsGenericType)
        {
            // Non-generic decorator: implements a specific IRequestHandler<ConcreteRequest, ConcreteResult>
            foreach (var iface in openDecoratorType.AllInterfaces)
            {
                if (SymbolEqualityComparer.Default.Equals(iface.OriginalDefinition, requestHandlerType))
                {
                    var ifaceRequestArg = iface.TypeArguments[0];
                    var ifaceResultArg = iface.TypeArguments[1];
                    if (SymbolEqualityComparer.Default.Equals(ifaceRequestArg, requestType) &&
                        SymbolEqualityComparer.Default.Equals(ifaceResultArg, resultType))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        // For generic decorators, check type parameter constraints
        var typeParams = openDecoratorType.TypeParameters;

        // We need to figure out which type parameter maps to TRequest and which to TResult.
        // Convention: the decorator implements IRequestHandler<TRequest, TResult> where
        // TRequest is the first type param constraint that involves IRequest/ICommand/IQuery.
        // Let's find the IRequestHandler interface implementation to determine the mapping.

        ITypeSymbol? requestTypeParam = null;
        ITypeSymbol? resultTypeParam = null;

        foreach (var iface in openDecoratorType.AllInterfaces)
        {
            if (SymbolEqualityComparer.Default.Equals(iface.OriginalDefinition, requestHandlerType))
            {
                requestTypeParam = iface.TypeArguments[0];
                resultTypeParam = iface.TypeArguments[1];
                break;
            }
        }

        if (requestTypeParam == null || resultTypeParam == null)
        {
            return false;
        }

        // Build a mapping from type parameters to concrete types
        var typeParamMap = new Dictionary<ITypeParameterSymbol, ITypeSymbol>(SymbolEqualityComparer.Default);

        if (requestTypeParam is ITypeParameterSymbol requestTp)
        {
            typeParamMap[requestTp] = requestType;
        }
        else if (!SymbolEqualityComparer.Default.Equals(requestTypeParam, requestType))
        {
            return false;
        }

        if (resultTypeParam is ITypeParameterSymbol resultTp)
        {
            typeParamMap[resultTp] = resultType;
        }
        else if (!SymbolEqualityComparer.Default.Equals(resultTypeParam, resultType))
        {
            return false;
        }

        // Check all constraints on all type parameters
        foreach (var tp in typeParams)
        {
            if (!typeParamMap.TryGetValue(tp, out var concreteType))
            {
                continue;
            }

            // Check each constraint
            foreach (var constraintType in tp.ConstraintTypes)
            {
                var substituted = SubstituteTypeParameters(constraintType, typeParamMap);
                if (!IsAssignableTo(compilation, concreteType, substituted))
                {
                    return false;
                }
            }

            // Check special constraints
            if (tp.HasReferenceTypeConstraint && !concreteType.IsReferenceType)
            {
                return false;
            }

            if (tp.HasValueTypeConstraint && !concreteType.IsValueType)
            {
                return false;
            }

            if (tp.HasConstructorConstraint)
            {
                if (concreteType is INamedTypeSymbol namedConcrete)
                {
                    var hasParameterlessCtor = namedConcrete.Constructors
                        .Any(c => c.Parameters.Length == 0 && c.DeclaredAccessibility == Accessibility.Public);
                    if (!hasParameterlessCtor)
                    {
                        return false;
                    }
                }
            }
        }

        return true;
    }

    private static ITypeSymbol SubstituteTypeParameters(
        ITypeSymbol type,
        Dictionary<ITypeParameterSymbol, ITypeSymbol> map)
    {
        if (type is ITypeParameterSymbol tp && map.TryGetValue(tp, out var substituted))
        {
            return substituted;
        }

        if (type is INamedTypeSymbol named && named.IsGenericType)
        {
            var args = named.TypeArguments;
            var newArgs = new ITypeSymbol[args.Length];
            bool changed = false;

            for (int i = 0; i < args.Length; i++)
            {
                newArgs[i] = SubstituteTypeParameters(args[i], map);
                if (!SymbolEqualityComparer.Default.Equals(newArgs[i], args[i]))
                {
                    changed = true;
                }
            }

            if (changed)
            {
                return named.OriginalDefinition.Construct(newArgs);
            }
        }

        return type;
    }

    private static bool IsAssignableTo(Compilation compilation, ITypeSymbol source, ITypeSymbol target)
    {
        if (SymbolEqualityComparer.Default.Equals(source, target))
        {
            return true;
        }

        // Check if source implements/extends target
        if (target is INamedTypeSymbol namedTarget)
        {
            if (namedTarget.TypeKind == TypeKind.Interface)
            {
                return source.AllInterfaces.Any(i =>
                    SymbolEqualityComparer.Default.Equals(i, namedTarget));
            }

            // Check base type chain
            var current = source.BaseType;
            while (current != null)
            {
                if (SymbolEqualityComparer.Default.Equals(current, namedTarget))
                {
                    return true;
                }

                current = current.BaseType;
            }
        }

        // Fallback: use Roslyn's conversion classification
        var conversion = compilation.ClassifyConversion(source, target);
        return conversion.IsImplicit;
    }

    private static IEnumerable<INamedTypeSymbol> GetAllTypes(INamespaceSymbol ns)
    {
        foreach (var type in ns.GetTypeMembers())
        {
            yield return type;
            foreach (var nested in GetNestedTypes(type))
            {
                yield return nested;
            }
        }

        foreach (var childNs in ns.GetNamespaceMembers())
        {
            foreach (var type in GetAllTypes(childNs))
            {
                yield return type;
            }
        }
    }

    private static IEnumerable<INamedTypeSymbol> GetNestedTypes(INamedTypeSymbol type)
    {
        foreach (var nested in type.GetTypeMembers())
        {
            yield return nested;
            foreach (var deepNested in GetNestedTypes(nested))
            {
                yield return deepNested;
            }
        }
    }

    private static string GetFullyQualifiedName(ITypeSymbol type)
    {
        return type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
    }

    private static string GenerateRegistrationCode(List<HandlerInfo> handlers)
    {
        var hasAnyConditionalDecorator = handlers.Any(h => h.ApplicableDecorators.Any(d => d.IsConditional));

        var sb = new StringBuilder();
        sb.AppendLine("// <auto-generated />");
        sb.AppendLine("#nullable enable");
        sb.AppendLine();
        sb.AppendLine("namespace softaware.Cqs.Generated;");
        sb.AppendLine();
        sb.AppendLine("/// <summary>");
        sb.AppendLine("/// Source-generated CQS service registrations.");
        sb.AppendLine("/// </summary>");
        sb.AppendLine("[global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)]");
        sb.AppendLine("internal static class CqsServiceRegistration");
        sb.AppendLine("{");
        sb.AppendLine("    public static void RegisterAll(global::Microsoft.Extensions.DependencyInjection.IServiceCollection services)");
        sb.AppendLine("    {");

        if (hasAnyConditionalDecorator)
        {
            // Register a default empty decorator registry (overridden by AddDecorators at runtime)
            sb.AppendLine("        global::Microsoft.Extensions.DependencyInjection.Extensions.ServiceCollectionDescriptorExtensions.TryAddSingleton<global::softaware.Cqs.DependencyInjection.CqsDecoratorRegistry>(");
            sb.AppendLine("            services,");
            sb.AppendLine("            new global::softaware.Cqs.DependencyInjection.CqsDecoratorRegistry());");
            sb.AppendLine();
        }

        foreach (var handler in handlers)
        {
            var requestFqn = GetFullyQualifiedName(handler.RequestType);
            var resultFqn = GetFullyQualifiedName(handler.ResultType);
            var handlerFqn = GetFullyQualifiedName(handler.HandlerType);
            var interfaceFqn = $"global::softaware.Cqs.IRequestHandler<{requestFqn}, {resultFqn}>";

            if (handler.ApplicableDecorators.Count == 0)
            {
                // Simple registration without decorators
                sb.AppendLine($"        global::Microsoft.Extensions.DependencyInjection.ServiceCollectionServiceExtensions.AddTransient<{interfaceFqn}>(");
                sb.AppendLine($"            services,");
                sb.AppendLine($"            static sp => global::Microsoft.Extensions.DependencyInjection.ActivatorUtilities.CreateInstance<{handlerFqn}>(sp));");
            }
            else
            {
                var handlerHasConditional = handler.ApplicableDecorators.Any(d => d.IsConditional);

                // Registration with decorator chain
                sb.AppendLine($"        global::Microsoft.Extensions.DependencyInjection.ServiceCollectionServiceExtensions.AddTransient<{interfaceFqn}>(");
                sb.AppendLine($"            services,");

                // Can only use 'static' lambda when there are no conditional decorators (no service lookups needed)
                var staticModifier = handlerHasConditional ? "" : "static ";
                sb.AppendLine($"            {staticModifier}sp =>");
                sb.AppendLine($"            {{");

                if (handlerHasConditional)
                {
                    sb.AppendLine($"                var __decoratorRegistry = global::Microsoft.Extensions.DependencyInjection.ServiceProviderServiceExtensions.GetRequiredService<global::softaware.Cqs.DependencyInjection.CqsDecoratorRegistry>(sp);");
                }

                sb.AppendLine($"                {interfaceFqn} current = global::Microsoft.Extensions.DependencyInjection.ActivatorUtilities.CreateInstance<{handlerFqn}>(sp);");

                foreach (var decoratorReg in handler.ApplicableDecorators)
                {
                    var closedDecoratorFqn = GetClosedDecoratorName(decoratorReg.Type, handler.RequestType, handler.ResultType);

                    if (decoratorReg.IsConditional)
                    {
                        var openDecoratorFqn = GetFullyQualifiedName(decoratorReg.Type);
                        sb.AppendLine($"                if (__decoratorRegistry.IsEnabled(typeof({openDecoratorFqn})))");
                        sb.AppendLine($"                {{");
                        sb.AppendLine($"                    current = global::Microsoft.Extensions.DependencyInjection.ActivatorUtilities.CreateInstance<{closedDecoratorFqn}>(sp, current);");
                        sb.AppendLine($"                }}");
                    }
                    else
                    {
                        sb.AppendLine($"                current = global::Microsoft.Extensions.DependencyInjection.ActivatorUtilities.CreateInstance<{closedDecoratorFqn}>(sp, current);");
                    }
                }

                sb.AppendLine($"                return current;");
                sb.AppendLine($"            }});");
            }

            sb.AppendLine();
        }

        // Register the generated request processor
        sb.AppendLine("        global::Microsoft.Extensions.DependencyInjection.ServiceCollectionServiceExtensions.AddTransient<global::softaware.Cqs.IRequestProcessor, global::softaware.Cqs.Generated.GeneratedRequestProcessor>(services);");
        sb.AppendLine("    }");
        sb.AppendLine("}");

        return sb.ToString();
    }

    private static string GetClosedDecoratorName(
        INamedTypeSymbol openDecoratorType,
        INamedTypeSymbol requestType,
        INamedTypeSymbol resultType)
    {
        if (!openDecoratorType.IsGenericType)
        {
            return GetFullyQualifiedName(openDecoratorType);
        }

        // Build the mapping from the decorator's IRequestHandler<TRequest, TResult> interface implementation
        var requestHandlerDef = openDecoratorType.AllInterfaces
            .FirstOrDefault(i => i.OriginalDefinition.ToDisplayString() == "softaware.Cqs.IRequestHandler<TRequest, TResult>");

        // Simple approach: construct the closed type with concrete request and result types
        // The decorator has type params that map to TRequest and TResult
        var typeParams = openDecoratorType.TypeParameters;
        var args = new ITypeSymbol[typeParams.Length];

        // Find which type param is TRequest and which is TResult by looking at the IRequestHandler implementation
        foreach (var iface in openDecoratorType.AllInterfaces)
        {
            if (iface.OriginalDefinition.MetadataName == "IRequestHandler`2" &&
                iface.ContainingNamespace?.ToDisplayString() == "softaware.Cqs")
            {
                for (int i = 0; i < typeParams.Length; i++)
                {
                    if (SymbolEqualityComparer.Default.Equals(iface.TypeArguments[0], typeParams[i]))
                    {
                        args[i] = requestType;
                    }
                    else if (SymbolEqualityComparer.Default.Equals(iface.TypeArguments[1], typeParams[i]))
                    {
                        args[i] = resultType;
                    }
                }
                break;
            }
        }

        // Fill any remaining unmapped params (shouldn't happen for well-formed decorators)
        for (int i = 0; i < args.Length; i++)
        {
            args[i] ??= requestType; // fallback
        }

        var closedType = openDecoratorType.Construct(args);
        return GetFullyQualifiedName(closedType);
    }

    private static string GenerateRequestProcessorCode(List<HandlerInfo> handlers)
    {
        var sb = new StringBuilder();
        sb.AppendLine("// <auto-generated />");
        sb.AppendLine("#nullable enable");
        sb.AppendLine();
        sb.AppendLine("namespace softaware.Cqs.Generated;");
        sb.AppendLine();
        sb.AppendLine("/// <summary>");
        sb.AppendLine("/// Source-generated static-dispatch request processor. No reflection at runtime.");
        sb.AppendLine("/// </summary>");
        sb.AppendLine("[global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)]");
        sb.AppendLine("internal sealed class GeneratedRequestProcessor : global::softaware.Cqs.IRequestProcessor");
        sb.AppendLine("{");
        sb.AppendLine("    private readonly global::System.IServiceProvider serviceProvider;");
        sb.AppendLine();
        sb.AppendLine("    public GeneratedRequestProcessor(global::System.IServiceProvider serviceProvider)");
        sb.AppendLine("        => this.serviceProvider = serviceProvider;");
        sb.AppendLine();
        sb.AppendLine("    public global::System.Threading.Tasks.Task<TResult> HandleAsync<TResult>(");
        sb.AppendLine("        global::softaware.Cqs.IRequest<TResult> request,");
        sb.AppendLine("        global::System.Threading.CancellationToken cancellationToken)");
        sb.AppendLine("    {");

        int handlerIndex = 0;
        foreach (var handler in handlers)
        {
            var requestFqn = GetFullyQualifiedName(handler.RequestType);
            var resultFqn = GetFullyQualifiedName(handler.ResultType);
            var interfaceFqn = $"global::softaware.Cqs.IRequestHandler<{requestFqn}, {resultFqn}>";
            var varName = $"r{handlerIndex}";
            handlerIndex++;

            sb.AppendLine($"        if (request is {requestFqn} {varName})");
            sb.AppendLine($"        {{");
            sb.AppendLine($"            var handler = global::Microsoft.Extensions.DependencyInjection.ServiceProviderServiceExtensions.GetRequiredService<{interfaceFqn}>(this.serviceProvider);");
            sb.AppendLine($"            return (global::System.Threading.Tasks.Task<TResult>)(object)handler.HandleAsync({varName}, cancellationToken);");
            sb.AppendLine($"        }}");
            sb.AppendLine();
        }

        sb.AppendLine("        throw new global::System.InvalidOperationException(");
        sb.AppendLine("            $\"No handler registered for request type '{request.GetType().Name}'. \" +");
        sb.AppendLine("            $\"Ensure the request type's assembly is included via IncludeTypesFrom(typeof(...)).\");");
        sb.AppendLine("    }");
        sb.AppendLine("}");

        return sb.ToString();
    }
}
