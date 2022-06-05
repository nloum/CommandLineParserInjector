using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CommandLine;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace CommandLineParserInjector;

public static class Extensions
{
    /// <summary>
    /// Runs the correct command line handler based on the command line arguments registered with dependency injection
    /// via <see cref="Extensions.AddCommandLineOptions"/>
    /// </summary>
    public static Task RunCommandLineAsync(this IHost host)
    {
        var runner = host.Services.GetService<ICommandLineRunner>();
        if (runner is null)
        {
            throw new InvalidOperationException(
                "Cannot run command line when there are no command line handlers specified.");
        }
        return runner.RunAsync();
    }

    /// <summary>
    /// If there are command line arguments, runs the correct command line handler. Otherwise, runs the services.
    /// </summary>
    public static Task RunServicesOrCommandLineAsync(this IHost host)
    {
        var args = host.Services.GetRequiredService<CommandLineArguments>();
        if (args.Value.Length > 0)
        {
            var runner = host.Services.GetRequiredService<ICommandLineRunner>();
            return runner.RunAsync();
        }

        return host.RunAsync();
    }

    /// <summary>
    /// If there are command line arguments, runs the correct command line handler. Then, runs the services whether command line
    /// arguments were specified or not.
    /// </summary>
    public static async Task RunCommandLineThenServicesAsync(this IHost host)
    {
        var args = host.Services.GetRequiredService<CommandLineArguments>();
        if (args.Value.Length > 0)
        {
            var runner = host.Services.GetRequiredService<ICommandLineRunner>();
            await runner.RunAsync();
        }

        await host.RunAsync();
    }

    /// <summary>
    /// Makes the specified command line arguments available via dependency injection
    /// </summary>
    /// <param name="services">The dependency injection container that is having services registered in it</param>
    /// <param name="args">The command line arguments</param>
    /// <param name="parser">The command line parser to use, if specified; otherwise, uses <see cref="Parser.Default"/></param>
    public static void AddCommandLineArguments(this IServiceCollection services, string[] args, Parser? parser = null)
    {
        services.AddSingleton(parser ?? Parser.Default);
        services.AddSingleton(new CommandLineArguments(args));
    }
    
    /// <summary>
    /// Makes the specified command line arguments available via dependency injection, and adds the specified strongly-typed
    /// object that will be used to receive command line options
    /// </summary>
    /// <param name="services">The dependency injection container that is having services registered in it</param>
    /// <param name="args">The command line arguments</param>
    /// <param name="parser">The command line parser to use, if specified; otherwise, uses <see cref="Parser.Default"/></param>
    public static void AddCommandLineCommand<TCommandLineOptions>(this IServiceCollection services, string[] args, Parser? parser = null)
        where TCommandLineOptions : class
    {
        services.AddCommandLineCommand<TCommandLineOptions>();
        services.AddSingleton(parser ?? Parser.Default);
        services.AddSingleton(new CommandLineArguments(args));
    }
    
    /// <summary>
    /// Makes the specified command line arguments available via dependency injection, and adds the specified strongly-typed
    /// object that will be used to receive command line options, and adds the service type that will be used to handle the
    /// command line options.
    /// </summary>
    /// <param name="services">The dependency injection container that is having services registered in it</param>
    /// <param name="args">The command line arguments</param>
    /// <param name="parser">The command line parser to use, if specified; otherwise, uses <see cref="Parser.Default"/></param>
    public static void AddCommandLineCommand<TCommandLineOptions, TCommandLineHandler>(this IServiceCollection services, string[] args, Parser? parser = null)
        where TCommandLineOptions : class
        where TCommandLineHandler : class, ICommandLineHandler<TCommandLineOptions>
    {
        services.AddCommandLineCommand<TCommandLineOptions>(args, parser);
        services.AddSingleton<ICommandLineHandler<TCommandLineOptions>, TCommandLineHandler>();
        services.AddSingleton<ICommandLineRunner, CommandLineRunner<TCommandLineOptions>>();
    }

    /// <summary>
    /// Adds a command line verb base to dependency injection. This makes it easy to inject a <see cref="TCommandLineVerbBase"/>
    /// into custom classes so they can access the verb no matter which verb was passed in. This is an advanced feature of
    /// the CommandLineParserInjector library, but it can be useful in scenarios, such as when certain options are available
    /// on all verbs. 
    /// </summary>
    /// <param name="services">The dependency injection container that is having services registered in it</param>
    public static void AddCommandLineVerbs<TCommandLineVerbBase>(this IServiceCollection services) where TCommandLineVerbBase : class
    {
        services.AddSingleton<ICommandLineHandler<TCommandLineVerbBase>, CommandLineVerbBaseHandler<TCommandLineVerbBase>>();
        services.AddSingleton<ICommandLineRunner>(di => (CommandLineVerbBaseHandler<TCommandLineVerbBase>)di.GetRequiredService<ICommandLineHandler<TCommandLineVerbBase>>());
        services.AddSingleton(di => new AnyVerb()
        {
            Value = di.GetRequiredService<TCommandLineVerbBase>(),
        });
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
    
    /// <summary>
    /// Makes it so you can execute multiple command line verbs from this command line application.
    /// </summary>
    /// <param name="services">The dependency injection container that is having services registered in it</param>
    public static void AddCommandLineVerbs(this IServiceCollection services)
    {
        services.AddCommandLineVerbs<object>();
    }

    /// <summary>
    /// Specifies the strongly-typed object type that will be used to receive command line parameters.
    /// </summary>
    /// <param name="services">The dependency injection container that is having services registered in it</param>
    /// <typeparam name="TCommandLineOptions">The strongly-typed object type that will be used to receive command line parameters</typeparam>
    public static void AddCommandLineCommand<TCommandLineOptions>(this IServiceCollection services)
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

    /// <summary>
    /// Specifies a command line verb type that will be used to receive command line parameters if this verb is the one
    /// that is specified.
    /// </summary>
    /// <param name="services">The dependency injection container that is having services registered in it</param>
    /// <typeparam name="TCommandLineVerbOptions">The strongly-typed object type that will be used to receive command line parameters
    /// if this verb is the one that is specified.</typeparam>
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

    /// <summary>
    /// Specifies a command line verb type that will be used to receive command line parameters if this verb is the one
    /// that is specified. Also specifies the handler that should be called when this verb is specified on the command line.
    /// </summary>
    /// <param name="services">The dependency injection container that is having services registered in it</param>
    /// <typeparam name="TCommandLineVerbOptions">The strongly-typed object type that will be used to receive command line parameters
    /// if this verb is the one that is specified.</typeparam>
    /// <typeparam name="TCommandLineVerbHandler">The handler that should be executed when this verb is specified on the
    /// command line.</typeparam>
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