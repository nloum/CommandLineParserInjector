#if (!enableImplicitUsings)
using System;
using System.Threading.Tasks;
#endif
using CommandLine;
using CommandLineParserInjector;

namespace CommandLineTemplate;

[Verb("verb1")]
public class Verb1Verb
{
    [Option('p', "path", HelpText = "A file path", Required = true)]
    public string Path { get; set; }
}

public class Verb1Handler : ICommandLineHandler<Verb1Verb>
{
    public async Task ExecuteAsync(Verb1Verb verb)
    {
        // TODO - add code here
    }
}
