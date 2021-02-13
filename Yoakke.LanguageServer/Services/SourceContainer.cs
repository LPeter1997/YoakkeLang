using OmniSharp.Extensions.LanguageServer.Protocol;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Yoakke.Text;

namespace Yoakke.LanguageServer.Services
{
    internal class SourceContainer
    {
        private readonly ConcurrentDictionary<DocumentUri, SourceFile> documents = new ConcurrentDictionary<DocumentUri, SourceFile>();

        public void Update(DocumentUri uri, string text)
        {
            var sourceFile = new SourceFile(uri.GetFileSystemPath(), text);
            documents.AddOrUpdate(uri, sourceFile, (otherUri, otherSourceFile) => sourceFile);
        }

        public void Remove(DocumentUri uri) => documents.TryRemove(uri, out var _);

        public bool TryGetValue(DocumentUri uri, out SourceFile sourceFile) => documents.TryGetValue(uri, out sourceFile);
    }
}
