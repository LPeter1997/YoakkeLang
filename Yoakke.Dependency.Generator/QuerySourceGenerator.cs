using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Yoakke.Dependency.Generator
{
    // TODO: Right now there is no way to clear input!
    // Maybe introduce an Unset variant?

    [Generator]
    public class QuerySourceGenerator : ISourceGenerator
    {
        // Collects interfaces with at least one attribute
        private class SyntaxReceiver : ISyntaxReceiver
        {
            public IList<InterfaceDeclarationSyntax> CandidateInterfaces { get; } = new List<InterfaceDeclarationSyntax>();

            public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
            {
                if (syntaxNode is InterfaceDeclarationSyntax interfaceDeclSyntax
                    && interfaceDeclSyntax.AttributeLists.Count > 0)
                {
                    CandidateInterfaces.Add(interfaceDeclSyntax);
                }
            }
        }

        private INamedTypeSymbol cancellationTokenSymbol;
        private INamedTypeSymbol eventHandlerSymbol;
        private INamedTypeSymbol queryChannelAttributeSymbol;

        public void Initialize(GeneratorInitializationContext context)
        {
            context.RegisterForSyntaxNotifications(() => new SyntaxReceiver());
        }

        public void Execute(GeneratorExecutionContext context)
        {
            if (!(context.SyntaxReceiver is SyntaxReceiver receiver)) return;

            var compilation = context.Compilation;

            // Get the symbol representing the QueryGroup attributes and other required symbols
            this.cancellationTokenSymbol = compilation.GetTypeByMetadataName(TypeNames.SystemCancellationToken);
            this.eventHandlerSymbol = compilation.GetTypeByMetadataName(TypeNames.SystemEventHandler);
            var queryGroupAttributeSymbol = compilation.GetTypeByMetadataName(TypeNames.QueryGroupAttribute);
            var inputQueryGroupAttributeSymbol = compilation.GetTypeByMetadataName(TypeNames.InputQueryGroupAttribute);
            this.queryChannelAttributeSymbol = compilation.GetTypeByMetadataName(TypeNames.QueryChannelAttribute);

            // Only keep the interfaces that are annotated with this
            // The ones that are annotated must be partial
            foreach (var syntax in receiver.CandidateInterfaces)
            {
                var model = compilation.GetSemanticModel(syntax.SyntaxTree);
                var symbol = model.GetDeclaredSymbol(syntax) as INamedTypeSymbol;
                // Filter interfaces without the query group attributes
                var isQuery = symbol.GetAttributes().Any(attr =>
                    SymbolEqualityComparer.Default.Equals(attr.AttributeClass, queryGroupAttributeSymbol));
                var isInputQuery = symbol.GetAttributes().Any(attr =>
                    SymbolEqualityComparer.Default.Equals(attr.AttributeClass, inputQueryGroupAttributeSymbol));
                if (!(isQuery || isInputQuery)) continue;
                // Try to generate code
                var generated = GenerateImplementation(isInputQuery, context, syntax, symbol);
                if (generated == null) continue;
                context.AddSource($"{symbol.Name}.Generated.cs", generated);
            }
        }

        private string? GenerateImplementation(
            bool isInput,
            GeneratorExecutionContext context,
            InterfaceDeclarationSyntax syntax,
            INamedTypeSymbol symbol)
        {
            if (!syntax.Modifiers.Any(Microsoft.CodeAnalysis.CSharp.SyntaxKind.PartialKeyword))
            {
                // No partial keyword, error to the user
                context.ReportDiagnostic(Diagnostic.Create(
                    Diagnostics.QueryGroupInterfaceMustBePartial,
                    syntax.GetLocation(),
                    syntax.Identifier.ValueText));
                return null;
            }
            if (!SymbolEqualityComparer.Default.Equals(symbol.ContainingSymbol, symbol.ContainingNamespace))
            {
                // Non-top-level declaration, error to the user
                context.ReportDiagnostic(Diagnostic.Create(
                    Diagnostics.QueryGroupInterfaceMustBeTopLevel,
                    syntax.GetLocation(),
                    syntax.Identifier.ValueText));
                return null;
            }
            // It's a proper interface, generate source code

            var namespaceName = symbol.ContainingNamespace.ToDisplayString();
            var interfaceName = symbol.Name;
            var accessibility = AccessibilityToString(symbol.DeclaredAccessibility);
            var baseInterfacePart = isInput ? $": {TypeNames.IInputQueryGroup}" : string.Empty;
            var contents = isInput
                ? GenerateInputQueryContents(context, symbol)
                : GenerateQueryContents(context, symbol);

            return $@"
namespace {namespaceName}
{{
    {accessibility} partial interface {interfaceName} {baseInterfacePart}
    {{
        {contents}
    }}
}}";
        }

        private string GenerateInputQueryContents(GeneratorExecutionContext context, INamedTypeSymbol symbol)
        {
            // Additional things to put inside the interface
            var additionalDeclarations = new StringBuilder();
            // Implementation methods for the proxy
            var proxyDefinitions = new StringBuilder();

            // Member declarations and implementations
            foreach (var member in IgnorePropertyAndEventMethods(symbol.GetMembers()))
            {
                if (member is IMethodSymbol methodSymbol)
                {
                    if (methodSymbol.Parameters.IsEmpty)
                    {
                        GenerateKeylessInputQuery(symbol.Name, additionalDeclarations, proxyDefinitions, member);
                    }
                    else
                    {
                        GenerateKeyedInputQuery(symbol.Name, additionalDeclarations, proxyDefinitions, methodSymbol);
                    }
                }
                else if (member is IPropertySymbol propertySymbol
                    && propertySymbol.GetMethod != null
                    && propertySymbol.SetMethod != null)
                {
                    GenerateKeylessInputQuery(symbol.Name, additionalDeclarations, proxyDefinitions, member);
                }
                else
                {
                    // Error
                    context.ReportDiagnostic(Diagnostic.Create(
                        Diagnostics.InputQueryGroupElementMustBePropertyOrMethod,
                        member.Locations.First(),
                        member.Locations.Skip(1),
                        symbol.Name,
                        member.Name));
                }
            }

            // Assemble the internals
            return $@"
public class Proxy : {symbol.Name} {{
    private {TypeNames.DependencySystem} dependencySystem;

    public Proxy({TypeNames.DependencySystem} dependencySystem) 
    {{
        this.dependencySystem = dependencySystem;
    }}

    {proxyDefinitions}
}}
{additionalDeclarations}";
        }

        private string GenerateQueryContents(GeneratorExecutionContext context, INamedTypeSymbol symbol)
        {
            // Implementation methods for the proxy
            var proxyDefinitions = new StringBuilder();
            // Additional initializations in the ctor
            var additionalInit = new StringBuilder();
            // All the clear calls for the storages
            var clearCalls = new StringBuilder();

            // Member declarations and implementations
            foreach (var member in IgnorePropertyAndEventMethods(symbol.GetMembers()))
            {
                bool generateClear = true;
                if (member is IMethodSymbol methodSymbol)
                {
                    if (   methodSymbol.Parameters.IsEmpty
                        || (methodSymbol.Parameters.Length == 1 && HasCancellationToken(methodSymbol)))
                    {
                        GenerateKeylessDerivedQuery(context, symbol, proxyDefinitions, additionalInit, member);
                    }
                    else
                    {
                        GenerateKeyedDerivedQuery(context, symbol, proxyDefinitions, additionalInit, methodSymbol);
                    }
                }
                else if (member is IPropertySymbol propertySymbol && propertySymbol.GetMethod != null)
                {
                    GenerateKeylessDerivedQuery(context, symbol, proxyDefinitions, additionalInit, member);
                }
                else if (member is IEventSymbol eventSymbol)
                {
                    GenerateQueryChannel(symbol.Name, proxyDefinitions, eventSymbol);
                    generateClear = false;
                }
                else
                {
                    // Error
                    context.ReportDiagnostic(Diagnostic.Create(
                        Diagnostics.QueryGroupElementMustBePropertyOrMethod,
                        member.Locations.First(),
                        member.Locations.Skip(1),
                        symbol.Name,
                        member.Name));
                }
                if (generateClear) clearCalls.AppendLine($"this.{member.Name}_storage.Clear(before);");
            }

            // Assemble the internals
            return $@"
public class Proxy : {symbol.Name} {{
    private {TypeNames.DependencySystem} dependencySystem;
    private {symbol.Name} implementation;

    public Proxy({TypeNames.DependencySystem} dependencySystem, {symbol.Name} implementation) 
    {{
        this.dependencySystem = dependencySystem;
        this.implementation = implementation;
        {additionalInit}
    }}

    public void Clear({TypeNames.Revision} before)
    {{
        {clearCalls}
    }}

    {proxyDefinitions}
}}";
        }

        private void GenerateKeylessInputQuery(
            string interfaceName,
            StringBuilder additionalDeclarations,
            StringBuilder proxyDefinitions,
            ISymbol querySymbol)
        {
            var accessibility = AccessibilityToString(querySymbol.DeclaredAccessibility);
            var storedType = (querySymbol is IMethodSymbol ms ? ms.ReturnType : ((IPropertySymbol)querySymbol).Type).ToDisplayString();
            if (querySymbol is IMethodSymbol)
            {
                // Method, generate extra declaration for setter
                additionalDeclarations.AppendLine($"{accessibility} void Set{querySymbol.Name}({storedType} value);");
            }

            // Generate storage
            proxyDefinitions.AppendLine($"private {TypeNames.IDependencyValue} {querySymbol.Name}_storage = new {TypeNames.InputDependencyValue}();");

            // Generate implementation
            var getterImpl = $"return this.{querySymbol.Name}_storage.GetValue<{storedType}>(this.dependencySystem);";
            var setterImpl = $"(({TypeNames.InputDependencyValue})this.{querySymbol.Name}_storage).SetValue(this.dependencySystem, value);";

            // Syntax depends on which one this is
            if (querySymbol is IMethodSymbol)
            {
                // Use method syntax
                proxyDefinitions.AppendLine($@"{accessibility} {storedType} {querySymbol.Name}() {{ {getterImpl} }}");
                proxyDefinitions.AppendLine($@"{accessibility} void Set{querySymbol.Name}({storedType} value) {{ {setterImpl} }}");
            }
            else
            {
                // Use property syntax
                proxyDefinitions.AppendLine($@"
{storedType} {interfaceName}.{querySymbol.Name}
{{
    get {{ {getterImpl} }}
    set {{ {setterImpl} }}
}}");
            }
        }

        private void GenerateKeyedInputQuery(
            string interfaceName,
            StringBuilder additionalDeclarations,
            StringBuilder proxyDefinitions,
            IMethodSymbol querySymbol)
        {
            var accessibility = AccessibilityToString(querySymbol.DeclaredAccessibility);
            var storedType = querySymbol.ReturnType;
            // Method, generate extra declaration for setter
            var setterKeyParams = string.Join(", ",
                querySymbol.Parameters
                    .Select(p => $"{p.Type.ToDisplayString()} {p.Name}")
                    .Append($"{storedType} value"));
            additionalDeclarations.AppendLine($"{accessibility} void Set{querySymbol.Name}({setterKeyParams});");

            // Generate storage
            var keyTypes = string.Join(", ", querySymbol.Parameters.Select(p => p.Type.ToDisplayString()));
            proxyDefinitions.AppendLine($"private {TypeNames.KeyValueCache} {querySymbol.Name}_storage = new {TypeNames.KeyValueCache}();");

            var getterKeyParams = string.Join(", ", querySymbol.Parameters.Select(p => $"{p.Type.ToDisplayString()} {p.Name}"));
            var keyParamNames = string.Join(", ", querySymbol.Parameters.Select(p => p.Name));

            // Generate getter
            // TODO: We could include keys here
            proxyDefinitions.AppendLine($@"
{accessibility} {storedType} {querySymbol.Name}({getterKeyParams}) {{
    return this.{querySymbol.Name}_storage.GetInput(({keyParamNames})).GetValue<{storedType}>(this.dependencySystem);
}}");
            // Generate setter
            proxyDefinitions.AppendLine($@"
{accessibility} void Set{querySymbol.Name}({setterKeyParams}) {{
    this.{querySymbol.Name}_storage.SetInput(this.dependencySystem, ({keyParamNames}), value);
}}");
        }

        private void GenerateQueryChannel(
            string interfaceName,
            StringBuilder proxyDefinitions,
            IEventSymbol channelSymbol)
        {
            var accessibility = AccessibilityToString(channelSymbol.DeclaredAccessibility);
            var definedType = channelSymbol.Type.ToDisplayString();
            proxyDefinitions.AppendLine($@"
{accessibility} event {definedType} {channelSymbol.Name}
{{
    add => this.implementation.{channelSymbol.Name} += value;
    remove => this.implementation.{channelSymbol.Name} -= value;
}}");
        }

        private void GenerateKeylessDerivedQuery(
            GeneratorExecutionContext context,
            INamedTypeSymbol interfac,
            StringBuilder proxyDefinitions,
            StringBuilder additionalInit,
            ISymbol querySymbol)
        {
            GenerateQueryChannelProxies(context, interfac, proxyDefinitions, additionalInit, querySymbol);

            var accessibility = AccessibilityToString(querySymbol.DeclaredAccessibility);
            var returnTypeSymbol = (querySymbol is IMethodSymbol ms ? ms.ReturnType : ((IPropertySymbol)querySymbol).Type);
            var isAsync = IsAwaitable(returnTypeSymbol, out var storedTypeSymbol);
            storedTypeSymbol = isAsync ? storedTypeSymbol : returnTypeSymbol;
            var hasCt = querySymbol is IMethodSymbol ms2 && HasCancellationToken(ms2);
            var returnType = returnTypeSymbol.ToDisplayString();
            var storedType = storedTypeSymbol.ToDisplayString();

            var callParams = hasCt ? "(system, cancellationToken)" : "system";
            var callPassArgs = hasCt ? "cancellationToken" : string.Empty;
            var callImpl = querySymbol is IPropertySymbol
                ? $"{callParams} => this.implementation.{querySymbol.Name}"
                : $"{callParams} => this.implementation.{querySymbol.Name}({callPassArgs})";

            // Generate storage
            proxyDefinitions.AppendLine($"private {TypeNames.IDependencyValue} {querySymbol.Name}_storage;");
            // Init storage
            //var delegateName = TypeNames.GetComputeDelegateName(isAsync, hasCt);
            additionalInit.AppendLine($@"
    this.{querySymbol.Name}_storage = new {TypeNames.DerivedDependencyValue}(this.{querySymbol.Name}_eventProxies, {TypeNames.DerivedDependencyValue}.ToAsyncCtDelegate({callImpl}));");

            var getterExtraArgs = hasCt ? ", cancellationToken" : string.Empty;
            var getterPostfix = isAsync ? "Async" : string.Empty;
            var getterImpl = $"return this.{querySymbol.Name}_storage.GetValue{getterPostfix}<{storedType}>(this.dependencySystem{getterExtraArgs});";

            if (querySymbol is IMethodSymbol)
            {
                // Use method syntax
                var getterExtraParams = hasCt ? $"{TypeNames.SystemCancellationToken} cancellationToken" : string.Empty;
                proxyDefinitions.AppendLine($@"{accessibility} {returnType} {querySymbol.Name}({getterExtraParams}) {{ {getterImpl} }}");
            }
            else
            {
                // Use property syntax
                proxyDefinitions.AppendLine($@"
{returnType} {interfac.Name}.{querySymbol.Name}
{{
    get {{ {getterImpl} }}
}}");
            }
        }

        private void GenerateKeyedDerivedQuery(
            GeneratorExecutionContext context,
            INamedTypeSymbol interfac,
            StringBuilder proxyDefinitions,
            StringBuilder additionalInit,
            IMethodSymbol querySymbol)
        {
            GenerateQueryChannelProxies(context, interfac, proxyDefinitions, additionalInit, querySymbol);

            var accessibility = AccessibilityToString(querySymbol.DeclaredAccessibility);
            var returnTypeSymbol = querySymbol.ReturnType;
            var isAsync = IsAwaitable(returnTypeSymbol, out var storedTypeSymbol);
            storedTypeSymbol = isAsync ? storedTypeSymbol : returnTypeSymbol;
            var hasCt = HasCancellationToken(querySymbol);
            var returnType = returnTypeSymbol.ToDisplayString();
            var storedType = storedTypeSymbol.ToDisplayString();

            // Generate storage
            proxyDefinitions.AppendLine($"private {TypeNames.KeyValueCache} {querySymbol.Name}_storage = new {TypeNames.KeyValueCache}();");

            var getterParams = string.Join(", ", querySymbol.Parameters.Select(p => $"{p.Type.ToDisplayString()} {p.Name}"));
            var keyParams = hasCt ? querySymbol.Parameters.Take(querySymbol.Parameters.Length - 1) : querySymbol.Parameters;
            var keyParamNames = string.Join(", ", keyParams.Select(p => p.Name));

            var callParams = hasCt ? "(system, cancellationToken)" : "system";
            var callExtraArgs = hasCt ? $", {querySymbol.Parameters.Last().Name}" : string.Empty;
            var callImpl = $"{callParams} => this.implementation.{querySymbol.Name}({keyParamNames}{callExtraArgs})";
            var getterPostfix = isAsync ? "Async" : string.Empty;
            //var delegateName = TypeNames.GetComputeDelegateName(isAsync, hasCt);
            var getterImpl = $@"
    return this.{querySymbol.Name}_storage.GetDerived(({keyParamNames}), this.{querySymbol.Name}_eventProxies, {TypeNames.DerivedDependencyValue}.ToAsyncCtDelegate({callImpl}))
        .GetValue{getterPostfix}<{storedType}>(this.dependencySystem{callExtraArgs});";

            // Generate getter
            proxyDefinitions.AppendLine($@"
{accessibility} {returnType} {querySymbol.Name}({getterParams}) {{ {getterImpl} }}");
        }

        private void GenerateQueryChannelProxies(
            GeneratorExecutionContext context,
            INamedTypeSymbol interfac,
            StringBuilder proxyDefinitions,
            StringBuilder additionalInit,
            ISymbol querySymbol)
        {
            //var interfaceName = interfac.Name;
            // We declare the event proxy array
            proxyDefinitions.AppendLine($"private {TypeNames.EventProxy}[] {querySymbol.Name}_eventProxies;");
            // Collect each event proxy instantiation here
            var eventProxyInits = new List<string>();
            // Generate implementations
            var queryChannelNames = GetRelevantQueryChannels(querySymbol);
            foreach (var member in interfac.GetMembers())
            {
                // Search for this member name in the channels
                var nameIndex = queryChannelNames.IndexOf(member.Name);
                // Not a channel name
                if (nameIndex == -1) continue;
                // Remove it from the unresolved channel names
                queryChannelNames.RemoveAt(nameIndex);
                // The member must be an event
                if (member is IEventSymbol eventSymbol)
                {
                    var eventType = eventSymbol.Type;
                    if (TryGetEventHandlerGenericType(eventType, out var argumentType))
                    {
                        var eventTypeName = eventType.ToDisplayString();
                        var argTypeName = argumentType.ToDisplayString();
                        // Ok, we can generate it
                        // Generate the event proxy instance
                        eventProxyInits.Add($@"new {TypeNames.EventProxy}(
    eventHandler => {{
        {eventTypeName} typedEventHandler = (sender, args) => eventHandler(sender, args);
        this.{member.Name} += typedEventHandler;
        return () => this.{member.Name} -= typedEventHandler;
    }},
    events => {{
        if (this.{member.Name} == null) return;
        foreach (var (sender, arg) in events) this.{member.Name}(sender, ({argTypeName})arg);
    }}
)");
                    }
                    else
                    {
                        // Error
                        context.ReportDiagnostic(Diagnostic.Create(
                            Diagnostics.QueryChannelEventMustBeEventHandler,
                            null,
                            member.Name));
                    }
                }
                else
                {
                    // Error, not an event
                    context.ReportDiagnostic(Diagnostic.Create(
                        Diagnostics.SpecifiedQueryChannelIsNotAnEventMember,
                        null,
                        member.Name));
                }
            }
            // Report unmatched ones as error
            foreach (var name in queryChannelNames)
            {
                context.ReportDiagnostic(Diagnostic.Create(
                    Diagnostics.NoMemberForQueryChannelName,
                    null,
                    name));
            }
            // Now that we have all the proxy initializers we can put together the proxy array in the constructor
            additionalInit.AppendLine($"this.{querySymbol.Name}_eventProxies = new {TypeNames.EventProxy}[] {{ {string.Join(", ", eventProxyInits)} }};");
        }

        private static bool IsAwaitable(ITypeSymbol symbol, out ITypeSymbol awaitedType)
        {
            awaitedType = null;
            foreach (var member in symbol.GetMembers())
            {
                if (member.Kind == SymbolKind.Method && member.Name == "GetAwaiter"
                    && member is IMethodSymbol getAwaiterMethod)
                {
                    var awaiterType = getAwaiterMethod.ReturnType;
                    var awaiterGetResult = awaiterType.GetMembers().First(m => m.Name == "GetResult");
                    if (awaiterGetResult is IMethodSymbol awaiterGetResultSym)
                    {
                        awaitedType = awaiterGetResultSym.ReturnType;
                        return true;
                    }
                }
            }
            return false;
        }

        private bool HasCancellationToken(IMethodSymbol methodSymbol) =>
               methodSymbol.Parameters.Length > 0
            && SymbolEqualityComparer.Default.Equals(this.cancellationTokenSymbol, methodSymbol.Parameters.Last().Type);

        private IList<string> GetRelevantQueryChannels(ISymbol symbol) => symbol.GetAttributes()
            .Where(attr => SymbolEqualityComparer.Default.Equals(attr.AttributeClass, queryChannelAttributeSymbol))
            .Select(attr => attr.ConstructorArguments.First().Value.ToString())
            .ToList();

        private bool TryGetEventHandlerGenericType(ITypeSymbol eventType, out ITypeSymbol argumentType)
        {
            argumentType = null;
            if (!(eventType is INamedTypeSymbol namedEventType)) return false;
            if (!SymbolEqualityComparer.Default.Equals(namedEventType.ConstructedFrom, eventHandlerSymbol)) return false;
            argumentType = namedEventType.TypeArguments.First();
            return true;
        }

        private static string AccessibilityToString(Accessibility accessibility) => accessibility switch
        {
            Accessibility.Public => "public",
            Accessibility.Internal => "internal",
            Accessibility.Protected => "protected",
            Accessibility.Private => "private",
            Accessibility.NotApplicable => string.Empty,
            _ => throw new NotImplementedException(),
        };

        private static IEnumerable<ISymbol> IgnorePropertyAndEventMethods(IEnumerable<ISymbol> symbols)
        {
            // Collect property names
            var propertyNames = new HashSet<string>();
            foreach (var propertyName in symbols.OfType<IPropertySymbol>().Select(p => p.Name))
            {
                propertyNames.Add(propertyName);
            }
            // Collect event names
            var eventNames = new HashSet<string>();
            foreach (var eventName in symbols.OfType<IEventSymbol>().Select(p => p.Name))
            {
                eventNames.Add(eventName);
            }
            return symbols.Where(sym =>
            {
                if (!(sym is IMethodSymbol methodSymbol)) return true;
                return !(
                        // Starts with get_ or set_ and property
                        ((   sym.Name.StartsWith("get_") || sym.Name.StartsWith("set_"))
                          && propertyNames.Contains(sym.Name.Substring(4)))
                          //Or starts with add_ or remove_ and is an event
                     || (    (sym.Name.StartsWith("add_") && eventNames.Contains(sym.Name.Substring(4)))
                          || (sym.Name.StartsWith("remove_") && eventNames.Contains(sym.Name.Substring(7))))
                );
            });
        }
    }
}
