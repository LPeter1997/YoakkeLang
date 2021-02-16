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
                        QueryGroupInterfaceMustBePartial,
                        syntax.GetLocation(),
                        syntax.Identifier.ValueText));
                return null;
            }
            // It's a proper interface, generate source code

            // TODO: For now as test we just inject a new method

            var namespaceName = symbol.ContainingNamespace.ToDisplayString();
            var interfaceName = symbol.Name;
            var accessibility = AccessibilityToString(symbol.DeclaredAccessibility);
            var baseInterfacePart = isInput ? ": Yoakke.Dependency.IInputQueryGroup" : string.Empty;
            var contents = isInput
                ? GenerateInputQueryContents(context, symbol)
                : GenerateQueryContents(symbol);

            var source = $@"
namespace {namespaceName}
{{
    {accessibility} partial interface {interfaceName} {baseInterfacePart}
    {{
        {contents}
    }}
}}
";

            return source;
        }

        private static string GenerateInputQueryContents(GeneratorExecutionContext context, INamedTypeSymbol symbol)
        {
            // Additional things to put inside the interface
            var additionalDeclarations = new StringBuilder();

            foreach (var member in symbol.GetMembers())
            {
                if (member is IMethodSymbol methodSymbol)
                {
                    // We need to create a set member
                    // For a member declaration <accessibility> <return-type> <name>(<t1> p1, <t2> p2, ... <tN> pN);
                    // We want to create a <accessibility> void Set<name>(<t1> p1, <t2> p2, ... <tN> pN, <return-type> value);
                    var methodAccessibility = AccessibilityToString(methodSymbol.DeclaredAccessibility);
                    var returnType = methodSymbol.ReturnType.ToDisplayString();
                    var parameterList = methodSymbol
                        .Parameters
                        .Select(p => $"{p.Type.ToDisplayString()} {p.Name}")
                        .Append($"{returnType} value");
                    var parameters = string.Join(", ", parameterList);
                    // Build out the declaration string
                    additionalDeclarations.Append($"{methodAccessibility} void Set{methodSymbol.Name}({parameters});");
                    // TODO: Implementation string
                }
                else if (member is IPropertySymbol propertySymbol 
                    && propertySymbol.GetMethod != null
                    && propertySymbol.SetMethod != null)
                {
                    // NOTE: We don't want additional declaration here
                    // TODO
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

            return additionalDeclarations.ToString();
        }

        private static string GenerateQueryContents(INamedTypeSymbol symbol)
        {
            // TODO
            return "";
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
