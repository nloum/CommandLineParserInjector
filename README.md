# CommandLineParserInjector

This library makes it easy to write C# command line apps that use dependency injection. To use it:

1. Install the template: `dotnet new --install CommandLineParserInjector.Template`
2. Make a directory for your new project
3. Inside the directory, run `dotnet new cli`

## Adding to existing project

1. Add a dependency on `CommandLineParserInjector`
2. Add a dependency on `Microsoft.Extensions.Hosting`
3. Use the following code as an example for your `Program.cs` file:

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

4. Or if you want your command line program to support multiple [verbs](https://github.com/commandlineparser/commandline/wiki/Verbs), you can do use this code:

```
using CommandLine;
using CommandLineParserInjector;

IHost host = Host.CreateDefaultBuilder()
    .ConfigureServices(services =>
    {
        services.AddCommandLineArguments(args);
        services.AddCommandLineVerb<AddVerb, AddHandler>();
        services.AddCommandLineVerb<CompleteVerb, CompleteHandler>();
    })
    .Build();

await host.RunCommandLineAsync();

[Verb("add", HelpText = "Add a new TODO item")]
public class AddVerb
{
    [Option('t', "todo", HelpText = "The ID of the TODO", Required = true)]
    public string TodoId { get; set; }
}

[Verb("complete", HelpText = "Mark an existing TODO item as completed")]
public class CompleteVerb
{
    [Option('t', "todo", HelpText = "The ID of the TODO", Required = true)]
    public string TodoId { get; set; }
}

public class AddHandler : ICommandLineHandler<AddVerb>
{
    public Task ExecuteAsync(AddVerb verb)
    {
        // TODO - code to add a TODO item goes here
        return Task.CompletedTask;
    }
}

public class CompleteHandler : ICommandLineHandler<CompleteVerb>
{
    public Task ExecuteAsync(CompleteVerb verb)
    {
        // TODO - code to mark a TODO item as complete goes here
        return Task.CompletedTask;
    }
}
```
