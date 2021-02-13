using MediatR;
using OmniSharp.Extensions.LanguageServer.Protocol;
using OmniSharp.Extensions.LanguageServer.Protocol.Client.Capabilities;
using OmniSharp.Extensions.LanguageServer.Protocol.Document;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using OmniSharp.Extensions.LanguageServer.Protocol.Server;
using OmniSharp.Extensions.LanguageServer.Protocol.Server.Capabilities;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Yoakke.Compiler.Compile;
using Yoakke.Compiler.Error;
using Yoakke.Compiler.Semantic;
using Yoakke.LanguageServer.Services;
using Yoakke.Text;

namespace Yoakke.LanguageServer.Handlers
{
    internal class TextDocumentSyncHandler : ITextDocumentSyncHandler
    {
        // TODO: We should switch to incremental sync kind later
        public TextDocumentSyncKind SyncKind => TextDocumentSyncKind.Full;

        private readonly ILanguageServerFacade server;
        private readonly SourceContainer sourceContainer;

        public TextDocumentSyncHandler(ILanguageServerFacade server, SourceContainer sourceContainer)
        {
            this.server = server;
            this.sourceContainer = sourceContainer;
        }

        public TextDocumentChangeRegistrationOptions GetRegistrationOptions() => new TextDocumentChangeRegistrationOptions
        {
            DocumentSelector = Globals.DocumentSelector,
            SyncKind = SyncKind,
        };

        TextDocumentRegistrationOptions IRegistration<TextDocumentRegistrationOptions>.GetRegistrationOptions() => new TextDocumentRegistrationOptions
        {
            DocumentSelector = Globals.DocumentSelector,
        };

        TextDocumentSaveRegistrationOptions IRegistration<TextDocumentSaveRegistrationOptions>.GetRegistrationOptions() => new TextDocumentSaveRegistrationOptions
        {
            DocumentSelector = Globals.DocumentSelector,
            IncludeText = false,
        };

        public TextDocumentAttributes GetTextDocumentAttributes(DocumentUri uri) => new TextDocumentAttributes(uri, "yoakke");

        public Task<Unit> Handle(DidChangeTextDocumentParams request, CancellationToken cancellationToken)
        {
            var uri = request.TextDocument.Uri;
            var text = request.ContentChanges.FirstOrDefault()?.Text;

            sourceContainer.Update(uri, text);

            PublishTestDiagnostics(uri);

            return Unit.Task;
        }

        public Task<Unit> Handle(DidOpenTextDocumentParams request, CancellationToken cancellationToken)
        {
            var uri = request.TextDocument.Uri;
            var text = request.TextDocument.Text;

            sourceContainer.Update(uri, text);

            PublishTestDiagnostics(uri);

            return Unit.Task;
        }

        public Task<Unit> Handle(DidCloseTextDocumentParams request, CancellationToken cancellationToken)
        {
            var uri = request.TextDocument.Uri;

            sourceContainer.Remove(uri);

            return Unit.Task;
        }

        public Task<Unit> Handle(DidSaveTextDocumentParams request, CancellationToken cancellationToken)
        {
            return Unit.Task;
        }

        public void SetCapability(SynchronizationCapability capability)
        {
        }

        private void PublishTestDiagnostics(DocumentUri uri)
        {
            var diagnostics = new Container<Diagnostic>();
            if (sourceContainer.TryGetValue(uri, out var sourceFile))
            {
                diagnostics = new Container<Diagnostic>(DiagnoseSourceFile(sourceFile)
                    .Select(DiagnosticTranslator.Translate));
            }
            server.TextDocument.PublishDiagnostics(new PublishDiagnosticsParams
            {
                Uri = uri,
                Diagnostics = diagnostics,
            });
        }

        private IList<ICompileError> DiagnoseSourceFile(SourceFile sourceFile)
        {
            // NOTE: For now we just parse and check syntax
            var result = new List<ICompileError>();
            // Create a dependency system
            var system = new DependencySystem("../stdlib");
            var symTab = system.SymbolTable;
            // Register for errors
            system.CompileError += (s, err) => result.Add(err);
            // From now on we do semantic checking
            // If at any phase we encounter errors, simply terminate

            // Parse the ast
            var ast = system.ParseAst(sourceFile);
            if (result.Count > 0) return result;

            // Do symbol resolution
            SymbolResolution.Resolve(symTab, ast);
            if (result.Count > 0) return result;

            // Finally type-check
            system.TypeCheck(ast);
            return result;
        }
    }
}
