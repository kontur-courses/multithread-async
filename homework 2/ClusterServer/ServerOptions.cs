using System;
using Fclp;

namespace Cluster
{
     public class ServerOptions
    {
        public int Port { get; set; }
        public string MethodName { get; set; }
        public int MethodDuration { get; set; }
        public bool Async { get; set; }
        public int Status { get; set; }

        public static bool TryGetArguments(string[] args, out ServerOptions parsedOptions)
        {
            var argumentsParser = new FluentCommandLineParser<ServerOptions>();
            argumentsParser.Setup(a => a.Port)
                .As(CaseType.CaseInsensitive, "p", "port")
                .Required();

            argumentsParser.Setup(a => a.MethodName)
                .As(CaseType.CaseInsensitive, "n", "name")
                .Required();

            argumentsParser.Setup(a => a.MethodDuration)
                .As(CaseType.CaseInsensitive, "d", "duration")
                .WithDescription("Server will return his response in <duration> ms")
                .Required();

            argumentsParser.Setup(a => a.Async)
                .As(CaseType.CaseInsensitive, "a", "async")
                .SetDefault(false);

            argumentsParser.Setup(a => a.Status)
                .As(CaseType.CaseInsensitive, "s", "status")
                .SetDefault(200);

            argumentsParser.SetupHelp("?", "h", "help")
                .Callback(text => Console.WriteLine(text));

            var parsingResult = argumentsParser.Parse(args);

            if (parsingResult.HasErrors)
            {
                argumentsParser.HelpOption.ShowHelp(argumentsParser.Options);
                parsedOptions = null;
                return false;
            }

            parsedOptions = argumentsParser.Object;
            return !parsingResult.HasErrors;
        }
    }
}