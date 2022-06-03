#if (!EnableImplicitUsings)
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
#endif
using CommandLine;
using CommandLineParserInjector;
using CommandLineTemplate;

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
            services.AddCommandLineOptions<SimpleCommandOptions, SimpleCommandHandler>(args);
#endif
#if (EnableCommandLineVerbs)
            services.AddCommandLineArguments(args);
            services.AddCommandLineVerb<Verb1Verb, Verb1Handler>();
            services.AddCommandLineVerb<Verb2Verb, Verb2Handler>();
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
        services.AddCommandLineOptions<SimpleOptions, SimpleHandler>(args);
#endif
#if (EnableCommandLineVerbs)
        services.AddCommandLineArguments(args);
        services.AddCommandLineVerb<Verb1Verb, Verb1Handler>();
        services.AddCommandLineVerb<Verb2Verb, Verb2Handler>();
#endif
    })
    .Build();

await host.RunCommandLineAsync();
#endif
