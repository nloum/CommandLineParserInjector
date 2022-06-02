using System.Threading.Tasks;

namespace CommandLineParserInjector;

public interface ICommandLineRunner
{
    Task RunAsync();
}