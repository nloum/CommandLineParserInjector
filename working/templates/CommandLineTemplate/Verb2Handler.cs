using CommandLine;
using CommandLineParserInjector;

namespace CommandLineTemplate;

[Verb("verb2")]
public class Verb2Verb
{
    [Option('p', "path", HelpText = "A file path", Required = true)]
    public string Path { get; set; }
}

public class Verb2Handler : ICommandLineHandler<Verb2Verb>
{
    public async Task ExecuteAsync(Verb2Verb verb)
    {
        // TODO - add code here
    }
}
