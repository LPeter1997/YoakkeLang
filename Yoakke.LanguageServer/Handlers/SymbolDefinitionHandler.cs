using OmniSharp.Extensions.LanguageServer.Protocol.Client.Capabilities;
using OmniSharp.Extensions.LanguageServer.Protocol.Document;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Yoakke.LanguageServer.Handlers
{
    internal class SymbolDefinitionHandler : IDefinitionHandler
    {
        public DefinitionRegistrationOptions GetRegistrationOptions() => new DefinitionRegistrationOptions
        {
            DocumentSelector = Globals.DocumentSelector,
            WorkDoneProgress = false,
        };

        public Task<LocationOrLocationLinks> Handle(DefinitionParams request, CancellationToken cancellationToken)
        {
            return Task.FromResult(new LocationOrLocationLinks());
        }

        public void SetCapability(DefinitionCapability capability)
        {
        }
    }
}
