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
    [Generator]
    public class QuerySourceGenerator : ISourceGenerator
    {
        private class SyntaxReceiver : ISyntaxReceiver
        {
            public IList<InterfaceDeclarationSyntax> CandidateInterfaces { get; set; } = new List<InterfaceDeclarationSyntax>();

            public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
            {
                if (syntaxNode is InterfaceDeclarationSyntax interfaceDeclSyntax
                    && interfaceDeclSyntax.AttributeLists.Count > 0)
                {
                    CandidateInterfaces.Add(interfaceDeclSyntax);
                }
            }
        }

        private static readonly DiagnosticDescriptor QueryGroupInterfaceMustBePartial = new DiagnosticDescriptor(
            id: "YKDEPENDENCYGEN001",
            title: "QueryGroup interface definitions must be partial",
            messageFormat: "QueryGroup interface '{0}' definition must be partial",
            category: "Yoakke.Dependency.Generator",
            DiagnosticSeverity.Error,
            isEnabledByDefault: true);

        private static readonly DiagnosticDescriptor QueryGroupInterfaceMustBeTopLevel = new DiagnosticDescriptor(
            id: "YKDEPENDENCYGEN002",
            title: "QueryGroup interface definitions must be top-level definition",
            messageFormat: "QueryGroup interface '{0}' definition must be a top-level definition",
            category: "Yoakke.Dependency.Generator",
            DiagnosticSeverity.Error,
            isEnabledByDefault: true);

        private static readonly DiagnosticDescriptor InputQueryMustBePropertyOrMethod = new DiagnosticDescriptor(
            id: "YKDEPENDENCYGEN003",
            title: "Input QueryGroup interface must only contain methods and get-set properties",
            messageFormat: "Input QueryGroup interface '{0}' must only contain methods and get-set properties, '{1}' is illegal",
            category: "Yoakke.Dependency.Generator",
            DiagnosticSeverity.Error,
            isEnabledByDefault: true);

        public void Initialize(GeneratorInitializationContext context)
        {
            context.RegisterForSyntaxNotifications(() => new SyntaxReceiver());
        }

        public void Execute(GeneratorExecutionContext context)
        {
            if (!(context.SyntaxReceiver is SyntaxReceiver receiver)) return;

            var compilation = context.Compilation;

            // Get the symbol representing the QueryGroup attributes
            var queryGroupAttributeSymbol = compilation.GetTypeByMetadataName("Yoakke.Dependency.QueryGroupAttribute");
            var inputQueryGroupAttributeSymbol = compilation.GetTypeByMetadataName("Yoakke.Dependency.InputQueryGroupAttribute");

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
                    QueryGroupInterfaceMustBePartial,
                    syntax.GetLocation(),
                    syntax.Identifier.ValueText));
                return null;
            }
            if (!SymbolEqualityComparer.Default.Equals(symbol.ContainingSymbol, symbol.ContainingNamespace))
            {
                // Non-top-level declaration, error to the user
                context.ReportDiagnostic(Diagnostic.Create(
                    QueryGroupInterfaceMustBeTopLevel,
                    syntax.GetLocation(),
                    syntax.Identifier.ValueText));
                return null;
            }
            // It's a proper interface, generate source code

            // TODO: For now as test we just inject a new method

            var namespaceName = symbol.ContainingNamespace.ToDisplayString();
            var interfaceName = symbol.Name;
            var accessibility = AccessibilityToString(symbol.DeclaredAccessibility);
            var baseInterfacePart = isInput ? ": Yoakke.Dependency.Internal.IInputQueryGroup" : string.Empty;
            var contents = isInput
                ? GenerateInputQueryContents(context, symbol)
                : GenerateQueryContents(symbol);

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

            // First we get all property names because we need to filter out property-generated methods
            var propertyNames = new HashSet<string>();
            foreach (var propertyName in symbol.GetMembers().OfType<IPropertySymbol>().Select(p => p.Name))
            {
                propertyNames.Add(propertyName);
            }

            // Member declarations and implementations
            foreach (var member in symbol.GetMembers())
            {
                if (member is IMethodSymbol methodSymbol)
                {
                    // We skip property methods
                    bool isPropertyMethod =
                           (member.Name.StartsWith("get_") || member.Name.StartsWith("set_"))
                        && propertyNames.Contains(member.Name.Substring(4));
                    if (!isPropertyMethod)
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
                        InputQueryMustBePropertyOrMethod,
                        member.Locations.First(),
                        member.Locations.Skip(1),
                        symbol.Name,
                        member.Name));
                }
            }

            // Assemble the internals
            return $@"
public class Proxy : {symbol.Name} {{
    private Yoakke.Dependency.DependencySystem dependencySystem;

    public Proxy(Yoakke.Dependency.DependencySystem dependencySystem) 
    {{
        this.dependencySystem = dependencySystem;
    }}

    {proxyDefinitions}
}}
{additionalDeclarations}";
        }

        private static string GenerateQueryContents(INamedTypeSymbol symbol)
        {
            return "";
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
            var storageType = $"Yoakke.Dependency.Internal.InputDependencyValue";
            proxyDefinitions.AppendLine($"private {storageType} {querySymbol.Name}_storage = new {storageType}();");

            // Generate implementation
            var getterImpl = $"return {querySymbol.Name}_storage.GetValue<{storedType}>();";
            var setterImpl = $"{querySymbol.Name}_storage.SetValue(this.dependencySystem, value);";

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

            // TODO: Generate proper storage
            var keyTypes = string.Join(", ", querySymbol.Parameters.Select(p => p.Type.ToDisplayString()));
            var storageType = "Yoakke.Dependency.Internal.DependencyValueStorage";
            proxyDefinitions.AppendLine($"private {storageType} {querySymbol.Name}_storage = new {storageType}();");

            var getterKeyParams = string.Join(", ", querySymbol.Parameters.Select(p => $"{p.Type.ToDisplayString()} {p.Name}"));
            var keyParamNames = string.Join(", ", querySymbol.Parameters.Select(p => p.Name));

            // TODO: Proper getter implementation
            // TODO: We could include the keys in the exception
            proxyDefinitions.AppendLine($@"
{accessibility} {storedType} {querySymbol.Name}({getterKeyParams}) {{ 
    return {querySymbol.Name}_storage.GetValue<{storedType}>(({keyParamNames}));
}}");
            // TODO: Proper setter implementation
            proxyDefinitions.AppendLine($@"
{accessibility} void Set{querySymbol.Name}({setterKeyParams}) {{
    {querySymbol.Name}_storage.SetValue(this.dependencySystem, ({keyParamNames}), value);
}}");
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
    }
}
