using Microsoft.Extensions.Logging;
using OmniSharp.Extensions.LanguageServer.Server;
using System;
using System.Threading.Tasks;
using LangServer = OmniSharp.Extensions.LanguageServer;

namespace Yoakke.LanguageServer
{
    class Program
    {
        static void Main(string[] args) => MainAsync(args).Wait();

        private static async Task MainAsync(string[] args)
        {
            var server = await LangServer.Server.LanguageServer.From(options => options
                .WithInput(Console.OpenStandardInput())
                .WithOutput(Console.OpenStandardOutput())
                .WithLoggerFactory(new LoggerFactory())
                .ConfigureLogging(builder => builder
                    .AddLanguageProtocolLogging()
                    .SetMinimumLevel(LogLevel.Debug)
                )
                .WithHandler<TextDocumentSyncHandler>()
            );

            await server.WaitForExit;
        }
    }
}
