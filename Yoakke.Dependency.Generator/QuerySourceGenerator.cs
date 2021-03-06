﻿using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Yoakke.Dependency.Generator
{
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

        public void Initialize(GeneratorInitializationContext context)
        {
            context.RegisterForSyntaxNotifications(() => new SyntaxReceiver());
        }

        public void Execute(GeneratorExecutionContext context)
        {
            if (!(context.SyntaxReceiver is SyntaxReceiver receiver)) return;

            var compilation = context.Compilation;

            // Get the symbol representing the QueryGroup attributes and other required symbols
            var queryGroupAttributeSymbol = compilation.GetTypeByMetadataName(TypeNames.QueryGroupAttribute);
            var inputQueryGroupAttributeSymbol = compilation.GetTypeByMetadataName(TypeNames.InputQueryGroupAttribute);
            this.cancellationTokenSymbol = compilation.GetTypeByMetadataName(TypeNames.SystemCancellationToken);

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
            foreach (var member in IgnorePropertyMethods(symbol.GetMembers()))
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

            // Member declarations and implementations
            foreach (var member in IgnorePropertyMethods(symbol.GetMembers()))
            {
                if (member is IMethodSymbol methodSymbol)
                {
                    if (methodSymbol.Parameters.IsEmpty)
                    {
                        GenerateKeylessDerivedQuery(symbol.Name, proxyDefinitions, additionalInit, member);
                    }
                    else
                    {
                        GenerateKeyedDerivedQuery(symbol.Name, proxyDefinitions, methodSymbol);
                    }
                }
                else if (member is IPropertySymbol propertySymbol && propertySymbol.GetMethod != null)
                {
                    GenerateKeylessDerivedQuery(symbol.Name, proxyDefinitions, additionalInit, member);
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

        private void GenerateKeylessDerivedQuery(
            string interfaceName,
            StringBuilder proxyDefinitions,
            StringBuilder additionalInit,
            ISymbol querySymbol)
        {
            var accessibility = AccessibilityToString(querySymbol.DeclaredAccessibility);
            var storedType = (querySymbol is IMethodSymbol ms ? ms.ReturnType : ((IPropertySymbol)querySymbol).Type).ToDisplayString();

            var callImpl = querySymbol is IPropertySymbol
                ? $"system => this.implementation.{querySymbol.Name}"
                : $"system => this.implementation.{querySymbol.Name}()";

            // Generate storage
            proxyDefinitions.AppendLine($"private {TypeNames.IDependencyValue} {querySymbol.Name}_storage;");
            // Init storage
            additionalInit.AppendLine($@"
    this.{querySymbol.Name}_storage = new {TypeNames.DerivedDependencyValue}(new {TypeNames.GetComputeDelegateName(false, false)}({callImpl}));");

            var getterImpl = $"return this.{querySymbol.Name}_storage.GetValue<{storedType}>(this.dependencySystem);";

            if (querySymbol is IMethodSymbol)
            {
                // Use method syntax
                proxyDefinitions.AppendLine($@"{accessibility} {storedType} {querySymbol.Name}() {{ {getterImpl} }}");
            }
            else
            {
                // Use property syntax
                proxyDefinitions.AppendLine($@"
{storedType} {interfaceName}.{querySymbol.Name}
{{
    get {{ {getterImpl} }}
}}");
            }
        }

        private void GenerateKeyedDerivedQuery(
            string interfaceName,
            StringBuilder proxyDefinitions,
            IMethodSymbol querySymbol)
        {
            var accessibility = AccessibilityToString(querySymbol.DeclaredAccessibility);
            var storedType = querySymbol.ReturnType;

            // Generate storage
            proxyDefinitions.AppendLine($"private {TypeNames.KeyValueCache} {querySymbol.Name}_storage = new {TypeNames.KeyValueCache}();");

            var getterKeyParams = string.Join(", ", querySymbol.Parameters.Select(p => $"{p.Type.ToDisplayString()} {p.Name}"));
            var keyParamNames = string.Join(", ", querySymbol.Parameters.Select(p => p.Name));

            var callImpl = $"system => this.implementation.{querySymbol.Name}({keyParamNames})";
            var getterImpl = $@"
    return this.{querySymbol.Name}_storage.GetDerived(({keyParamNames}), new {TypeNames.GetComputeDelegateName(false, false)}({callImpl}))
        .GetValue<{storedType}>(this.dependencySystem);";

            // Generate getter
            proxyDefinitions.AppendLine($@"
{accessibility} {storedType} {querySymbol.Name}({getterKeyParams}) {{ {getterImpl} }}");
        }

        private static bool IsAsynchronous(IMethodSymbol methodSymbol) => methodSymbol.ReturnType
            .GetMembers()
            .Any(m => m.Kind == SymbolKind.Method && m.Name == "GetAwaiter");

        private bool HasCancellationToken(IMethodSymbol methodSymbol) =>
               methodSymbol.Parameters.Length > 0
            && SymbolEqualityComparer.Default.Equals(this.cancellationTokenSymbol, methodSymbol.Parameters.Last().Type);

        private static string AccessibilityToString(Accessibility accessibility) => accessibility switch
        {
            Accessibility.Public => "public",
            Accessibility.Internal => "internal",
            Accessibility.Protected => "protected",
            Accessibility.Private => "private",
            Accessibility.NotApplicable => string.Empty,
            _ => throw new NotImplementedException(),
        };

        private static IEnumerable<ISymbol> IgnorePropertyMethods(IEnumerable<ISymbol> symbols)
        {
            var propertyNames = new HashSet<string>();
            foreach (var propertyName in symbols.OfType<IPropertySymbol>().Select(p => p.Name))
            {
                propertyNames.Add(propertyName);
            }
            return symbols.Where(sym =>
            {
                if (!(sym is IMethodSymbol methodSymbol)) return true;
                return !((sym.Name.StartsWith("get_") || sym.Name.StartsWith("set_"))
                       && propertyNames.Contains(sym.Name.Substring(4)));
            });
        }
    }
}
