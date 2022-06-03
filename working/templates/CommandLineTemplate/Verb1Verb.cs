using CommandLine;

namespace CommandLineTemplate;

[Verb("verb1")]
public class Verb1Verb
#if (inlineHandlers)
    : VerbBase
#endif
{
    [Option('p', "path", HelpText = "A file path", Required = true)]
    public string Path { get; set; }
}