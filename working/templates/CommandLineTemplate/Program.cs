#if (!enableImplicitUsings)
using System;
using Microsoft.Extensions.Hosting;
#endif
using CommandLine;
using CommandLineParserInjector;
using CommandLineTemplate;

#if(enableSerilog)
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
#if (enableCommand && !inlineHandlers)
            services.AddCommandLineOptions<SimpleCommandOptions, SimpleCommandHandler>(args);
#endif
#if (enableCommand && inlineHandlers)
            services.AddCommandLineOptions<SimpleCommandOptions>(args);
#endif
#if (enableVerbs && !inlineHandlers)
            services.AddCommandLineArguments(args);
            services.AddCommandLineVerb<Verb1Verb, Verb1Handler>();
            services.AddCommandLineVerb<Verb2Verb, Verb2Handler>();
#endif
#if (enableVerbs && inlineHandlers)
            services.AddCommandLineArguments(args);
            services.AddCommandLineVerbBase<VerbBase>();
            services.AddCommandLineVerb<Verb1Verb>();
            services.AddCommandLineVerb<Verb2Verb>();
#endif
        })
        .Build();

#if(!inlineHandlers)
    await host.RunCommandLineAsync();
#else
#if(enableCommand)
    var options = host.Services.GetRequiredService<SimpleCommandOptions>();
    // TODO - add code here
#else
    var options = host.Services.GetRequiredService<VerbBase>();
    // TODO - add code here
#endif
#endif
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
#if (enableCommand && !inlineHandlers)
        services.AddCommandLineOptions<SimpleCommandOptions, SimpleCommandHandler>(args);
#endif
#if (enableCommand && inlineHandlers)
        services.AddCommandLineOptions<SimpleCommandOptions>(args);
#endif
#if (enableVerbs && !inlineHandlers)
        services.AddCommandLineArguments(args);
        services.AddCommandLineVerb<Verb1Verb, Verb1Handler>();
        services.AddCommandLineVerb<Verb2Verb, Verb2Handler>();
#endif
#if (enableVerbs && inlineHandlers)
        services.AddCommandLineArguments(args);
        services.AddCommandLineVerbBase<VerbBase>();
        services.AddCommandLineVerb<Verb1Verb>();
        services.AddCommandLineVerb<Verb2Verb>();
#endif
    })
    .Build();

await host.RunCommandLineAsync();
#endif
