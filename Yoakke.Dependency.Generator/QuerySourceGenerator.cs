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

        public void Initialize(GeneratorInitializationContext context)
        {
            context.RegisterForSyntaxNotifications(() => new SyntaxReceiver());
        }

        public void Execute(GeneratorExecutionContext context)
        {
            if (!(context.SyntaxReceiver is SyntaxReceiver receiver)) return;

            var compilation = context.Compilation;

            // Get the symbol representing the QueryGroup attribute
            var queryGroupAttributeSymbol = compilation.GetTypeByMetadataName("Yoakke.Dependency.QueryGroupAttribute");

            // Only keep the interfaces that are annotated with this
            // The ones that are annotated must be partial
            foreach (var syntax in receiver.CandidateInterfaces)
            {
                var model = compilation.GetSemanticModel(syntax.SyntaxTree);
                var symbol = model.GetDeclaredSymbol(syntax) as INamedTypeSymbol;
                // Filter interfaces without the query group attribute
                if (!symbol.GetAttributes().Any(attr =>
                    SymbolEqualityComparer.Default.Equals(attr.AttributeClass, queryGroupAttributeSymbol)))
                {
                    continue;
                }
                // Try to generate code
                var generated = GenerateImplementation(context, syntax, symbol);
                if (generated == null) continue;
                context.AddSource($"{symbol.Name}.Generated.cs", generated);
            }
        }

        private string? GenerateImplementation(
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

            var source = $@"
namespace {namespaceName}
{{
    {accessibility} partial interface {interfaceName} 
    {{
        public void ThisIsInjected();
    }}
}}
";

            return source;
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
