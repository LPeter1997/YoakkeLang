﻿using MediatR;
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
using Yoakke.Compiler.Error;

namespace Yoakke.LanguageServer
{
    internal class TextDocumentSyncHandler : ITextDocumentSyncHandler
    {
        // TODO: We should switch to incremental sync kind later
        public TextDocumentSyncKind SyncKind => TextDocumentSyncKind.Full;
        public readonly DocumentSelector DocumentSelector = new DocumentSelector(new DocumentFilter
        {
            Pattern = "**/*.yk",
        });

        private readonly ILanguageServerFacade server;
        private readonly ConcurrentDictionary<string, string> documents = new ConcurrentDictionary<string, string>();

        public TextDocumentSyncHandler(ILanguageServerFacade server)
        {
            this.server = server;
        }

        public TextDocumentChangeRegistrationOptions GetRegistrationOptions() => new TextDocumentChangeRegistrationOptions
        {
            DocumentSelector = DocumentSelector,
            SyncKind = SyncKind,
        };

        TextDocumentRegistrationOptions IRegistration<TextDocumentRegistrationOptions>.GetRegistrationOptions() => new TextDocumentRegistrationOptions
        {
            DocumentSelector = DocumentSelector,
        };

        TextDocumentSaveRegistrationOptions IRegistration<TextDocumentSaveRegistrationOptions>.GetRegistrationOptions() => new TextDocumentSaveRegistrationOptions
        {
            DocumentSelector = DocumentSelector,
            IncludeText = false,
        };

        public TextDocumentAttributes GetTextDocumentAttributes(DocumentUri uri) => new TextDocumentAttributes(uri, "yoakke");

        public Task<Unit> Handle(DidChangeTextDocumentParams request, CancellationToken cancellationToken)
        {
            var documentPath = request.TextDocument.Uri.ToString();
            var text = request.ContentChanges.FirstOrDefault()?.Text;

            documents.AddOrUpdate(documentPath, text, (k, v) => text);

            PublishTestDiagnostics(documentPath);

            return Unit.Task;
        }

        public Task<Unit> Handle(DidOpenTextDocumentParams request, CancellationToken cancellationToken)
        {
            var documentPath = request.TextDocument.Uri.ToString();
            var text = request.TextDocument.Text;

            documents.AddOrUpdate(documentPath, text, (k, v) => text);

            PublishTestDiagnostics(documentPath);

            return Unit.Task;
        }

        public Task<Unit> Handle(DidCloseTextDocumentParams request, CancellationToken cancellationToken)
        {
            var documentPath = request.TextDocument.Uri.ToString();

            documents.TryRemove(documentPath, out var _);

            return Unit.Task;
        }

        public Task<Unit> Handle(DidSaveTextDocumentParams request, CancellationToken cancellationToken)
        {
            return Unit.Task;
        }

        public void SetCapability(SynchronizationCapability capability)
        {
        }

        private void PublishTestDiagnostics(string documentPath)
        {
            var diagnostics = new Container<Diagnostic>();
            if (documents.TryGetValue(documentPath, out var sourceText))
            {
                diagnostics = new Container<Diagnostic>(DiagnoseSourceFile(documentPath, sourceText)
                    .Select(DiagnosticTranslator.Translate));
            }
            server.TextDocument.PublishDiagnostics(new PublishDiagnosticsParams
            {
                Uri = documentPath,
                Diagnostics = diagnostics,
            });
        }

        private IList<ICompileError> DiagnoseSourceFile(string documentPath, string text)
        {
            // NOTE: For now we just parse and check syntax
            var result = new List<ICompileError>();
            var sourceFile = new Text.SourceFile(documentPath, text);
            var lexer = new Syntax.Lexer(sourceFile);
            lexer.SyntaxError += (sender, err) => result.Add(new SyntaxError(err));
            var parser = new Syntax.Parser(lexer.Lex());
            parser.SyntaxError += (sender, err) => result.Add(new SyntaxError(err));
            parser.ParseFile();
            return result;
        }
    }
}
