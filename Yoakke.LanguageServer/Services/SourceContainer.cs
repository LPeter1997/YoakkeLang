using OmniSharp.Extensions.LanguageServer.Protocol;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Yoakke.Compiler.Compile;
using Yoakke.Text;

namespace Yoakke.LanguageServer.Services
{
    internal class SourceContainer
    {
        private readonly ConcurrentDictionary<DocumentUri, SourceText> documents = new ConcurrentDictionary<DocumentUri, SourceText>();

        public void Update(DocumentUri uri, string text)
        {
            var sourceFile = new SourceText(uri.GetFileSystemPath(), text);
            documents.AddOrUpdate(uri, sourceFile, (otherUri, otherSourceFile) => sourceFile);
        }

        public void Remove(DocumentUri uri) => documents.TryRemove(uri, out var _);

        public bool TryGetValue(DocumentUri uri, [MaybeNullWhen(false)] out SourceText sourceFile) => 
            documents.TryGetValue(uri, out sourceFile);
    }
}
