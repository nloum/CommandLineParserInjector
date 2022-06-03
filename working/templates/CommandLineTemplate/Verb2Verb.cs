using CommandLine;

namespace CommandLineTemplate;

[Verb("verb2")]
public class Verb2Verb
#if (inlineHandlers)
    : VerbBase
#endif
{
    [Option('p', "path", HelpText = "A file path", Required = true)]
    public string Path { get; set; }
}