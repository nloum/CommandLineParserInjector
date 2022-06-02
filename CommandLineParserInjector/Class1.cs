using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommandLine;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace CommandLineParserInjector;

public interface ICommandLineHandler<in TCommandLineOptions>
{
    Task ExecuteAsync(TCommandLineOptions verb);
}

public interface ICommandLineRunner
{
	Task RunAsync();
}

public class CommandLineRunner<TCommandLineVerbBase> : ICommandLineRunner
    where TCommandLineVerbBase : class
{
    private readonly ICommandLineHandler<TCommandLineVerbBase> _handler;
    private readonly IServiceProvider _services;
    private readonly ILogger _logger;

    public CommandLineRunner(ICommandLineHandler<TCommandLineVerbBase> handler, IServiceProvider services, ILoggerFactory loggerFactory)
    {
	    _handler = handler;
	    _services = services;
	    _logger = loggerFactory.CreateLogger<CommandLineRunner<TCommandLineVerbBase>>();
    }

    public Task RunAsync()
    {
	    var verb = _services.GetRequiredService<TCommandLineVerbBase>();
	    return _handler.ExecuteAsync(verb);
    }
}

public class CommandLineVerbBaseHandler<TCommandLineVerbBase> : ICommandLineHandler<TCommandLineVerbBase>, ICommandLineRunner
    where TCommandLineVerbBase : class
{
    private readonly CommandLineVerbDescriptor[] _verbDescriptors;
    private readonly IServiceProvider _services;
    private readonly ILogger _logger;

    public CommandLineVerbBaseHandler(
        IEnumerable<CommandLineVerbDescriptor> verbDescriptors,
        ILoggerFactory loggerFactory,
        IServiceProvider services)
    {
        _verbDescriptors = verbDescriptors.ToArray();
        _services = services;
        _logger = loggerFactory.CreateLogger<CommandLineVerbBaseHandler<TCommandLineVerbBase>>();
    }

    public Task RunAsync()
    {
	    var verb = _services.GetRequiredService<TCommandLineVerbBase>();
	    return ExecuteAsync(verb);
    }

    public Task ExecuteAsync(TCommandLineVerbBase verb)
    {
        foreach (var verbDescriptor in _verbDescriptors)
        {
            if (verb.GetType() == verbDescriptor.Type)
            {
                if (verbDescriptor.ServiceType is not null)
                {
                    var service = _services.GetRequiredService(verbDescriptor.ServiceType);
                    var executeAsync = service.GetType().GetMethod("ExecuteAsync");
                    if (executeAsync is null)
                    {
                        _logger.LogError("Failed to find the ExecuteAsync method on command line verb handler {Type}", 
	                        verbDescriptor.ServiceType.GetCSharpTypeName());
                        return Task.CompletedTask;
                    }
                    var task = (Task)executeAsync.Invoke(service, new object[] { verb });
                    return task;
                }
                else
                {
                     _logger.LogError("No command line verb handler specified for verb. Try using AddCommandLineVerb<{VerbType}, MyCommandLineVerbHandler> instead of AddCommandLineVerb<>.",
	                     verb.GetType().GetCSharpTypeName());
                    return Task.CompletedTask;
                }
            }
        }
        
        _logger.LogError("Unregistered verb type. Try using AddCommandLineVerb<{VerbType}, MyCommandLineVerbHandler>.", verb.GetType().GetCSharpTypeName());
        return Task.CompletedTask;
    }
}

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

internal static class InternalExtensions
{
	/// <summary>
    /// Converts a <see cref="Type"/> to its C# name. E.g., if you pass in typeof(<see cref="IEnumerable{int}"/>) this
    /// will return "IEnumerable<int>".
    /// This is useful mainly in exception error messages.
    /// </summary>
    /// <param name="type">The type to get a C# description of.</param>
    /// <returns>The string representation of a human-friendly type name</returns>
    public static string GetCSharpTypeName( this Type type, Type[] genericParameters, bool includeNamespaces = false ) {
	    if ( type == typeof( long ) ) {
		    return "long";
	    }

	    if ( type == typeof( int ) ) {
		    return "int";
	    }

	    if ( type == typeof( short ) ) {
		    return "short";
	    }

	    if ( type == typeof( ulong ) ) {
		    return "ulong";
	    }

	    if ( type == typeof( uint ) ) {
		    return "uint";
	    }

	    if ( type == typeof( ushort ) ) {
		    return "ushort";
	    }

	    if ( type == typeof( byte ) ) {
		    return "byte";
	    }

	    if ( type == typeof( sbyte ) ) {
		    return "sbyte";
	    }

	    if ( type == typeof( string ) ) {
		    return "string";
	    }

	    if ( type == typeof( void ) ) {
		    return "void";
	    }

		var baseName = new StringBuilder();
		var typeName = includeNamespaces ? type.Namespace + "." + type.Name : type.Name;
	    if ( type.Name.Contains( "`" ) ) {
		    baseName.Append( typeName.Substring( 0, typeName.IndexOf( "`" ) ) );
	    } else {
		    baseName.Append( typeName );
	    }

	    if ( genericParameters.Any() ) {
		    baseName.Append( "<" );
		    for ( var i = 0; i < genericParameters.Length; i++ ) {
			    baseName.Append( GetCSharpTypeName( genericParameters[i], includeNamespaces ) );
			    if ( i + 1 < genericParameters.Length ) {
				    baseName.Append( ", " );
			    }
		    }

		    baseName.Append( ">" );
	    }

	    return baseName.ToString();
    }

	/// <summary>
	/// Converts a <see cref="Type"/> to its C# name. E.g., if you pass in typeof(<see cref="IEnumerable{int}"/>) this
	/// will return "IEnumerable<int>".
	/// This is useful mainly in exception error messages.
	/// </summary>
	/// <param name="type">The type to get a C# description of.</param>
	/// <returns>The string representation of a human-friendly type name</returns>
	public static string GetCSharpTypeName( this Type type, bool includeNamespaces = false ) {
	    if ( type == typeof(long) ) {
		    return "long";
	    }

	    if ( type == typeof( int ) ) {
		    return "int";
	    }

		if ( type == typeof(short) ) {
		    return "short";
	    }

		if ( type == typeof( ulong ) ) {
			return "ulong";
		}

		if ( type == typeof( uint ) ) {
			return "uint";
		}

		if ( type == typeof( ushort ) ) {
			return "ushort";
		}

		if ( type == typeof( byte ) ) {
			return "byte";
		}

		if ( type == typeof( sbyte ) ) {
			return "sbyte";
		}

		if ( type == typeof( string ) ) {
			return "string";
		}

		if ( type == typeof( void ) ) {
			return "void";
		}

		var baseName = new StringBuilder();
		var typeName = includeNamespaces ? type.Namespace + "." + type.Name : type.Name;
		if ( type.Name.Contains( "`" ) ) {
			baseName.Append( typeName.Substring( 0, typeName.IndexOf( "`" ) ) );
		} else {
			baseName.Append( typeName );
		}

		if ( type.GenericTypeArguments.Any() ) {
		    baseName.Append( "<" );
		    for ( var i = 0; i < type.GenericTypeArguments.Length; i++) {
			    baseName.Append( GetCSharpTypeName( type.GenericTypeArguments[i], includeNamespaces ) );
			    if ( i + 1 < type.GenericTypeArguments.Length ) {
				    baseName.Append( ", " );
			    }
		    }

		    baseName.Append(">");
	    }

	    return baseName.ToString();
    }
}