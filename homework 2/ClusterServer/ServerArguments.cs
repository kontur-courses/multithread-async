using System;
using Fclp;

namespace ClusterServer
{
    class ServerArguments
    {
        public int Port { get; set; }
        public string MethodName { get; set; }
        public int MethodDuration { get; set; }
        public bool Async { get; set; }

        public static bool TryGetArguments(string[] args, out ServerArguments parsedArguments)
        {
            var argumentsParser = new FluentCommandLineParser<ServerArguments>();
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

            argumentsParser.SetupHelp("?", "h", "help")
                .Callback(text => Console.WriteLine(text));

            var parsingResult = argumentsParser.Parse(args);

            if (parsingResult.HasErrors)
            {
                argumentsParser.HelpOption.ShowHelp(argumentsParser.Options);
                parsedArguments = null;
                return false;
            }

            parsedArguments = argumentsParser.Object;
            return !parsingResult.HasErrors;
        }
    }
}