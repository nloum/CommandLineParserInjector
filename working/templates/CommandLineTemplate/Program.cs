#if (!enableImplicitUsings)
using System;
using Microsoft.Extensions.Hosting;
#endif
using CommandLineParserInjector;
using CommandLineTemplate;
#if (inlineHandlers)
using Microsoft.Extensions.DependencyInjection;
#endif

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
#endif
#if(inlineHandlers && enableCommand)
    var options = host.Services.GetRequiredService<SimpleCommandOptions>();
    // TODO - add code here
#endif
#if(inlineHandlers && enableVerbs)
    var verb = host.Services.GetRequiredService<VerbBase>();
    if (verb is Verb1Verb verb1)
    {
        // TODO - add code here
    }
    else if (verb is Verb2Verb verb2)
    {
        // TODO - add code here
    }
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

#if(!inlineHandlers)
await host.RunCommandLineAsync();
#endif
#if(inlineHandlers && enableCommand)
var options = host.Services.GetRequiredService<SimpleCommandOptions>();
// TODO - add code here
#endif
#if(inlineHandlers && enableVerbs)
var verb = host.Services.GetRequiredService<VerbBase>();
if (verb is Verb1Verb verb1)
{
    // TODO - add code here
}
else if (verb is Verb2Verb verb2)
{
    // TODO - add code here
}
#endif
#endif
