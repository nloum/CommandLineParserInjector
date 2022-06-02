using System.Collections.Generic;
using System.Linq;
using CommandLine;
using Microsoft.Extensions.DependencyInjection;

namespace CommandLineParserInjector;

public static class Extensions
{
    public static void AddCommandLineArguments(this IServiceCollection services, string[] args, Parser? parser = null)
    {
        services.AddSingleton(parser ?? Parser.Default);
        services.AddSingleton(new CommandLineArguments(args));
    }
    
    public static void AddCommandLineOptions<TCommandLineOptions>(this IServiceCollection services, string[] args, Parser? parser = null)
        where TCommandLineOptions : class
    {
        services.AddCommandLineOptions<TCommandLineOptions>();
        services.AddSingleton(parser ?? Parser.Default);
        services.AddSingleton(new CommandLineArguments(args));
    }
    
    public static void AddCommandLineOptions<TCommandLineOptions, TCommandLineHandler>(this IServiceCollection services, string[] args, Parser? parser = null)
        where TCommandLineOptions : class
        where TCommandLineHandler : class, ICommandLineHandler<TCommandLineOptions>
    {
        services.AddCommandLineOptions<TCommandLineOptions>(args, parser);
        services.AddSingleton<ICommandLineHandler<TCommandLineOptions>, TCommandLineHandler>();
        services.AddSingleton<ICommandLineRunner, CommandLineRunner<TCommandLineOptions>>();
    }

    public static void AddCommandLineVerbBase<TCommandLineVerbBase>(this IServiceCollection services) where TCommandLineVerbBase : class
    {
        services.AddSingleton<ICommandLineHandler<TCommandLineVerbBase>, CommandLineVerbBaseHandler<TCommandLineVerbBase>>();
        services.AddSingleton<ICommandLineRunner>(di => (CommandLineVerbBaseHandler<TCommandLineVerbBase>)di.GetRequiredService<ICommandLineHandler<TCommandLineVerbBase>>());
        services.AddSingleton(di =>
        {
            var verbDescriptors = di.GetRequiredService<IEnumerable<CommandLineVerbDescriptor>>().Select(x => x.Type)
                .ToArray();
            var args = di.GetRequiredService<CommandLineArguments>();
            var parser = di.GetRequiredService<Parser>();
            var result = parser.ParseArguments(args.Value, verbDescriptors);
            return (TCommandLineVerbBase) result.Value;
        });
    }
    
    public static void AddCommandLineOptions<TCommandLineOptions>(this IServiceCollection services)
        where TCommandLineOptions : class
    {
        services.AddSingleton(di =>
        {
            var args = di.GetRequiredService<CommandLineArguments>().Value;
            var parser = di.GetRequiredService<Parser>();
            var result = parser.ParseArguments<TCommandLineOptions>(args);
            if (result.Value is not null)
            {
                return result.Value;
            }

            return null;
        });
    }

    public static void AddCommandLineVerb<TCommandLineVerbOptions>(this IServiceCollection services)
        where TCommandLineVerbOptions : class
    {
        services.AddSingleton(new CommandLineVerbDescriptor(typeof(TCommandLineVerbOptions), null));
        services.AddSingleton(di =>
        {
            var parser = di.GetRequiredService<Parser>();
            var args = di.GetRequiredService<CommandLineArguments>();
            var result = parser.ParseArguments(args.Value, typeof(TCommandLineVerbOptions));
            return result.Value as TCommandLineVerbOptions;
        });
    }
    
    public static void AddCommandLineVerb<TCommandLineVerbOptions, TCommandLineVerbHandler>(this IServiceCollection services)
        where TCommandLineVerbOptions : class
        where TCommandLineVerbHandler : class, ICommandLineHandler<TCommandLineVerbOptions>
    {
        services.AddSingleton(new CommandLineVerbDescriptor(typeof(TCommandLineVerbOptions), typeof(TCommandLineVerbHandler)));
        services.AddSingleton<TCommandLineVerbHandler>();
        services.AddSingleton<ICommandLineHandler<TCommandLineVerbOptions>>(di => di.GetRequiredService<TCommandLineVerbHandler>());
        services.AddSingleton(di =>
        {
            var parser = di.GetRequiredService<Parser>();
            var args = di.GetRequiredService<CommandLineArguments>();
            var result = parser.ParseArguments(args.Value, typeof(TCommandLineVerbOptions));
            return result.Value as TCommandLineVerbOptions;
        });
    }
}