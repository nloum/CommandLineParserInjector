namespace CommandLineParserInjector;

/// <summary>
/// A class that contains the verb object, no matter which verb was specified on the command line
/// </summary>
public class AnyVerb
{
    // The verb object, no matter which verb was specified on the command line
    public object Value { get; init; }
}
