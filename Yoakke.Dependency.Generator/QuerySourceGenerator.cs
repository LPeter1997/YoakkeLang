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
            messageFormat: "QueryGroup interface '{0}' definitions must be partial",
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
            foreach (var interfaceSyntax in receiver.CandidateInterfaces)
            {
                var model = compilation.GetSemanticModel(interfaceSyntax.SyntaxTree);
                var symbol = model.GetDeclaredSymbol(interfaceSyntax) as ITypeSymbol;
                // Filter interfaces without the query group attribute
                if (!symbol.GetAttributes().Any(attr =>
                    SymbolEqualityComparer.Default.Equals(attr.AttributeClass, queryGroupAttributeSymbol)))
                {
                    continue;
                }
                // It is marked with the attribute, now let's check that it's partial, as it must be
                if (!interfaceSyntax.Modifiers.Any(Microsoft.CodeAnalysis.CSharp.SyntaxKind.PartialKeyword))
                {
                    // No partial keyword, error to the user
                    context.ReportDiagnostic(Diagnostic.Create(
                        QueryGroupInterfaceMustBePartial,
                        interfaceSyntax.GetLocation(),
                        interfaceSyntax.Identifier.ValueText));
                }
            }
        }
    }
}
