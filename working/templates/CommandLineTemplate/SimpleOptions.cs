using CommandLine;

namespace CommandLineTemplate;

public class SimpleOptions
{
    [Option('p', "path", HelpText = "A file path", Required = true)]
    public string Path { get; set; }
}
