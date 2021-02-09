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
            );

            await server.WaitForExit;
        }
    }
}
