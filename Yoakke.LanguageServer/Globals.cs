using OmniSharp.Extensions.LanguageServer.Protocol;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yoakke.LanguageServer
{
    internal static class Globals
    {
        public static readonly DocumentSelector DocumentSelector = new DocumentSelector(new DocumentFilter
        {
            Pattern = "**/*.yk",
        });
    }
}
