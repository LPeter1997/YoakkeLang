using OmniSharp.Extensions.LanguageServer.Protocol.Client.Capabilities;
using OmniSharp.Extensions.LanguageServer.Protocol.Document;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Yoakke.Compiler.Compile;
using Yoakke.Compiler.Semantic;
using Yoakke.LanguageServer.Services;
using Yoakke.Syntax.Ast;
using Yoakke.Text;

namespace Yoakke.LanguageServer.Handlers
{
    // TODO: Fix this

    internal class SymbolDefinitionHandler : IDefinitionHandler
    {
        private readonly SourceContainer sourceContainer;

        public SymbolDefinitionHandler(SourceContainer sourceContainer)
        {
            this.sourceContainer = sourceContainer;
        }

        public DefinitionRegistrationOptions GetRegistrationOptions() => new DefinitionRegistrationOptions
        {
            DocumentSelector = Globals.DocumentSelector,
            WorkDoneProgress = false,
        };

        public Task<LocationOrLocationLinks> Handle(DefinitionParams request, CancellationToken cancellationToken)
        {
            if (sourceContainer.TryGet(request.TextDocument.Uri, out var sourceFile))
            {
                return Task.Run(() => FindDefinition(sourceFile, Translator.Translate(request.Position)));
            }
            else
            {
                return Task.FromResult(new LocationOrLocationLinks());
            }
        }

        public void SetCapability(DefinitionCapability capability)
        {
        }

        private LocationOrLocationLinks FindDefinition(SourceText sourceFile, Text.Position position)
        {
            // TODO: A bit redundant to parse again, we need to store ASTs too
            // We could do that in the dependency system or something
            // Also we are instantiating multiple dependency systems

            // Create a dependency system
            var system = new DependencySystem("../stdlib");
            var symTab = system.SymbolTable;
            var ast = system.ParseAst(sourceFile);
            SymbolResolution.Resolve(symTab, ast);

            var node = FindByPosition.Find(ast, position);
            // NOTE: We wouldn't need the check here, but ReferredSymbol throws...
            // We should avoid that
            if (node is Expression.Identifier ident)
            {
                var symbol = symTab.ReferredSymbol(ident);
                if (symbol.Definition != null && symbol.Definition.ParseTreeNode != null)
                {
                    var treeNode = symbol.Definition.ParseTreeNode;
                    return new LocationOrLocationLinks(
                        new LocationOrLocationLink(Translator.TranslateLocation(treeNode.Span)));
                }
            }

            return new LocationOrLocationLinks();
        }
    }
}
