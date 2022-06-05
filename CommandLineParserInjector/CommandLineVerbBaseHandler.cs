using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace CommandLineParserInjector;

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
        var verb = (TCommandLineVerbBase)_services.GetRequiredService<AnyVerb>().Value;
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
                    throw new InvalidOperationException(
                        $"No command line verb handler specified for verb. Try using AddCommandLineVerb<{verb.GetType().GetCSharpTypeName()}, MyCommandLineVerbHandler> instead of AddCommandLineVerb<>.");
                    return Task.CompletedTask;
                }
            }
        }

        _logger.LogError("Unregistered verb type. Try using AddCommandLineVerb<{VerbType}, MyCommandLineVerbHandler>.", verb.GetType().GetCSharpTypeName());
        
        throw new InvalidOperationException(
            $"Unregistered verb type. Try using AddCommandLineVerb<{verb.GetType().GetCSharpTypeName()}, MyCommandLineVerbHandler>.");
    }
}