#if (!enableImplicitUsings)
using System;
using System.Threading.Tasks;
#endif
using CommandLineParserInjector;

namespace CommandLineTemplate;

public class Verb1Handler : ICommandLineHandler<Verb1Verb>
{
    public async Task ExecuteAsync(Verb1Verb verb)
    {
        // TODO - add code here
    }
}
