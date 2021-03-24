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
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Yoakke.Compiler.Compile;
using Yoakke.Compiler.Error;
using Yoakke.Compiler.Semantic;
using Yoakke.Compiler.Services;
using Yoakke.LanguageServer.Services;
using Yoakke.Syntax.Error;
using Yoakke.Text;

namespace Yoakke.LanguageServer.Handlers
{
    internal class TextDocumentSyncHandler : ITextDocumentSyncHandler
    {
        public TextDocumentSyncKind SyncKind => TextDocumentSyncKind.Incremental;

        private readonly ILanguageServerFacade server;
        private readonly CompilerServices compilerServices;
        private readonly SourceContainer sourceContainer;

        public TextDocumentSyncHandler(
            ILanguageServerFacade server,
            CompilerServices compilerServices,
            SourceContainer sourceContainer)
        {
            this.server = server;
            this.compilerServices = compilerServices;
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

        public Task<Unit> Handle(DidOpenTextDocumentParams request, CancellationToken cancellationToken)
        {
            var uri = request.TextDocument.Uri;
            var text = request.TextDocument.Text;

            sourceContainer.Add(uri, text);

            SetCompilerInput(uri);
            PublishDiagnostics(uri);

            return Unit.Task;
        }

        public Task<Unit> Handle(DidCloseTextDocumentParams request, CancellationToken cancellationToken)
        {
            var uri = request.TextDocument.Uri;

            sourceContainer.Remove(uri);

            // TODO: SetInput for compiler
            // We can't do that yet, we can't reliably delete individual entries

            return Unit.Task;
        }

        public Task<Unit> Handle(DidSaveTextDocumentParams request, CancellationToken cancellationToken)
        {
            return Unit.Task;
        }

        public Task<Unit> Handle(DidChangeTextDocumentParams request, CancellationToken cancellationToken)
        {
            var uri = request.TextDocument.Uri;
            foreach (var change in request.ContentChanges)
            {
                Debug.Assert(change.Range is not null);
                var range = Translator.Translate(change.Range);
                sourceContainer.Edit(uri, range, change.Text);
            }

            SetCompilerInput(uri);
            PublishDiagnostics(uri);

            return Unit.Task;
        }

        public void SetCapability(SynchronizationCapability capability)
        {
        }

        private void SetCompilerInput(DocumentUri uri)
        {
            var path = uri.GetFileSystemPath();
            var text = sourceContainer.Get(uri);
            compilerServices.Input.SetSourceText(path, text);
        }

        private void PublishDiagnostics(DocumentUri uri)
        {
            var diagnostics = new Container<Diagnostic>();
            if (sourceContainer.TryGet(uri, out var sourceFile))
            {
#pragma warning disable CS8620 // Argument cannot be used for parameter due to differences in the nullability of reference types.
                diagnostics = new Container<Diagnostic>(DiagnoseSourceFile(sourceFile)
                    .Where(diag => diag != null)
                    .Select(Translator.Translate));
#pragma warning restore CS8620 // Argument cannot be used for parameter due to differences in the nullability of reference types.
            }
            server.TextDocument.PublishDiagnostics(new PublishDiagnosticsParams
            {
                Uri = uri,
                Diagnostics = diagnostics,
            });
        }

        private IList<ICompileError> DiagnoseSourceFile(SourceText sourceFile)
        {
            // NOTE: For now we just check syntax
            var errors = new List<ICompileError>();

            EventHandler<ISyntaxError> errorHandler = (sender, args) => errors.Add(new SyntaxError(args));
            compilerServices.Syntax.OnError += errorHandler;
            var ast = compilerServices.Syntax.ParseFileToDesugaredAst(sourceFile.Path);
            compilerServices.Syntax.OnError -= errorHandler;

            return errors;
        }
    }
}
