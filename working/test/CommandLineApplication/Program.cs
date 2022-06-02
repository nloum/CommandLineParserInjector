using CommandLine;
using CommandLineParserInjector;
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
            services.AddCommandLineArguments(args);
            services.AddCommandLineVerb<AddVerb, AddHandler>();
            services.AddCommandLineVerb<CompleteVerb, CompleteHandler>();
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
