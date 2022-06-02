using System;
using CommandLine;
using CommandLineParserInjector;
#if(EnableSerilog)
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    IHost host = Host.CreateDefaultBuilder()
        .UseSerilog((context, services, configuration) => configuration
            .WriteTo.Console()
            .ReadFrom.Configuration(context.Configuration)
            .ReadFrom.Services(services))
        .ConfigureServices(services =>
        {
#if (EnableSingleCommand)
            services.AddCommandLineOptions<SimpleCommandLineOptions, SimpleCommandLineHandler>(args);
#endif
#if (EnableCommandLineVerbs)
            services.AddCommandLineArguments(args);
            services.AddCommandLineVerb<AddVerb, AddHandler>();
            services.AddCommandLineVerb<CompleteVerb, CompleteHandler>();
#endif
        })
        .Build();

    await host.RunCommandLineAsync();
}
catch (Exception ex)
{
    Log.Fatal(ex, "An unhandled exception occured during bootstrapping");
}
finally
{
    Log.CloseAndFlush();
}
#else
IHost host = Host.CreateDefaultBuilder()
    .ConfigureServices(services =>
    {
#if (EnableSingleCommand)
        services.AddCommandLineOptions<SimpleCommandLineOptions, SimpleCommandLineHandler>(args);
#endif
#if (EnableCommandLineVerbs)
        services.AddCommandLineArguments(args);
        services.AddCommandLineVerb<AddVerb, AddHandler>();
        services.AddCommandLineVerb<CompleteVerb, CompleteHandler>();
#endif
    })
    .Build();

await host.RunCommandLineAsync();
#endif

#if (EnableCommandLineVerbs)
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
        throw new NotImplementedException();
    }
}

public class CompleteHandler : ICommandLineHandler<CompleteVerb>
{
    public Task ExecuteAsync(CompleteVerb verb)
    {
        // TODO - code to mark a TODO item as complete goes here
        throw new NotImplementedException();
    }
}
#endif
#if (EnableSingleCommand)
public class SimpleCommandLineOptions
{
    [Option('p', "path", HelpText = "A simple file path string property", Required = true)]
    public string FilePath { get; set; }
}

public class SimpleCommandLineHandler : ICommandLineHandler<SimpleCommandLineOptions>
{
    public async Task ExecuteAsync(SimpleCommandLineOptions verb)
    {
        // TODO - code to process SimpleCommandLineOptions goes here
        throw new NotImplementedException();
    }
}
#endif