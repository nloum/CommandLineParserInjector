using System.Threading.Tasks;

namespace CommandLineParserInjector;

public interface ICommandLineHandler<in TCommandLineOptions>
{
    Task ExecuteAsync(TCommandLineOptions verb);
}