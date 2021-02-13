using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OmniSharp.Extensions.LanguageServer.Protocol.Document;
using OmniSharp.Extensions.LanguageServer.Server;
using System;
using System.Threading.Tasks;
using Yoakke.LanguageServer.Handlers;
using Yoakke.LanguageServer.Services;
using LangServer = OmniSharp.Extensions.LanguageServer;
using TextDocumentSyncHandler = Yoakke.LanguageServer.Handlers.TextDocumentSyncHandler;

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
                .WithServices(ConfigureServices)
                .WithHandler<TextDocumentSyncHandler>()
                .WithHandler<SymbolDefinitionHandler>()
            );

            await server.WaitForExit;
        }

        private static void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<SourceContainer>();
        }
    }
}
