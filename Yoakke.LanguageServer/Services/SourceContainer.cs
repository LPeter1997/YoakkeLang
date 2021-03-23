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
using Range = Yoakke.Text.Range;

namespace Yoakke.LanguageServer.Services
{
    internal class SourceContainer
    {
        private readonly ConcurrentDictionary<DocumentUri, SourceText> sources = new ConcurrentDictionary<DocumentUri, SourceText>();

        public void Add(DocumentUri uri, string initialContent) =>
            sources.TryAdd(uri, new SourceText(uri.GetFileSystemPath(), initialContent));

        public void Remove(DocumentUri uri) => sources.TryRemove(uri, out var _);

        public void Edit(DocumentUri uri, Range range, string newContent)
        {
            var sourceText = Get(uri);
            sourceText.Edit(range, newContent);
        }

        public bool TryGet(DocumentUri uri, [MaybeNullWhen(false)] out SourceText sourceText) => 
            sources.TryGetValue(uri, out sourceText);

        public SourceText Get(DocumentUri uri)
        {
            if (TryGet(uri, out var sourceText)) return sourceText;
            throw new KeyNotFoundException();
        }
    }
}
