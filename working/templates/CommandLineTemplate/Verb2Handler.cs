#if (!enableImplicitUsings)
using System;
using System.Threading.Tasks;
#endif
using CommandLineParserInjector;

namespace CommandLineTemplate;

public class Verb2Handler : ICommandLineHandler<Verb2Verb>
{
    public async Task ExecuteAsync(Verb2Verb verb)
    {
        // TODO - add code here
    }
}
