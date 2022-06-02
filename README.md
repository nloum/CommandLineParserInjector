# CommandLineParserInjector

This library makes it easy to write C# command line apps that use dependency injection. To use it:

1. Add a dependency on `CommandLineParserInjector`
2. Put the following code in your `Program.cs` file:

```
IHost host = Host.CreateDefaultBuilder()
    .ConfigureServices(services =>
    {
        services.AddCommandLineOptions<SimpleCommandLineOptions, SimpleCommandLineHandler>(args);
        services.AddSingleton(test.Object);
    })
    .Build();

var runner = host.Services.GetService<ICommandLineRunner>();
runner.Should().NotBeNull();

await runner.RunCommandLineAsync();

public class SimpleCommandLineOptions
{
    [Option('p', "path", HelpText = "A simple file path string property", Required = true)]
    public string FilePath { get; set; }
}

public class SimpleCommandLineHandler : ICommandLineHandler<SimpleCommandLineOptions>
{
    public async Task ExecuteAsync(SimpleCommandLineOptions verb)
    {
        // Command line processing goes here
    }
}
```

3. Or if you want your command line program to support multiple [verbs](https://github.com/commandlineparser/commandline/wiki/Verbs), you can do this:

```
var args = new[]{"verb1", "-i", "test.txt"};
IHost host = Host.CreateDefaultBuilder()
    .ConfigureServices(services =>
    {
        services.AddCommandLineArguments(args);
        services.AddCommandLineVerb<CommandLineVerb1, CommandLineHandler1>();
        services.AddCommandLineVerb<CommandLineVerb2, CommandLineHandler2>();
        services.AddSingleton(test.Object);
    })
    .Build();

var runner = host.Services.GetRequiredService<ICommandLineRunner>();

await hot.RunCommandLineAsync();

[Verb("verb1")]
public class CommandLineVerb1
{
    [Option('i', "input", HelpText = "A simple string property", Required = true)]
    public string InputPath { get; set; }

    public override string FilePath => InputPath;
}

[Verb("verb2")]
public class CommandLineVerb2
{
    [Option('o', "output", HelpText = "A simple string property", Required = true)]
    public string OutputPath { get; set; }

    public override string FilePath => OutputPath;
}

public class CommandLineHandler1 : ICommandLineHandler<CommandLineVerb1>
{
    public Task ExecuteAsync(CommandLineVerb1 verb)
    {
        // TODO - put code to process a verb1 command here
        return Task.CompletedTask;
    }
}

public class CommandLineHandler2 : ICommandLineHandler<CommandLineVerb2>
{
    public Task ExecuteAsync(CommandLineVerb2 verb)
    {
        // TODO - put code to process a verb2 command here
        return Task.CompletedTask;
    }
}
```
