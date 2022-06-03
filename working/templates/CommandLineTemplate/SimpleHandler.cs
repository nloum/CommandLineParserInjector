using CommandLine;
using CommandLineParserInjector;

namespace CommandLineTemplate;

public class SimpleOptions
{
    [Option('p', "path", HelpText = "A file path", Required = true)]
    public string Path { get; set; }
}

public class SimpleHandler : ICommandLineHandler<SimpleOptions>
{
    public async Task ExecuteAsync(SimpleOptions verb)
    {
        // TODO - add code here
    }
}
