using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace CommandLineParserInjector;

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