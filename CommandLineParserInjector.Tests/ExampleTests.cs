using System.Collections.Generic;
using System.Threading.Tasks;
using CommandLine;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace CommandLineParserInjector.Tests;

public class SimpleCommandLineOptions
{
    [Option('p', "path", HelpText = "A simple file path string property", Required = true)]
    public string FilePath { get; set; }
}

public abstract class CustomVerbBase
{
    public abstract string FilePath { get; }
}

[Verb("verb1")]
public class CommandLineVerb1 : CustomVerbBase
{
    [Option('i', "input", HelpText = "A simple string property", Required = true)]
    public string InputPath { get; set; }

    public override string FilePath => InputPath;
}

[Verb("verb2")]
public class CommandLineVerb2 : CustomVerbBase
{
    [Option('o', "output", HelpText = "A simple string property", Required = true)]
    public string OutputPath { get; set; }

    public override string FilePath => OutputPath;
}

public class CommandLineHandler1 : ICommandLineHandler<CommandLineVerb1>
{
    private readonly ITest _test;

    public CommandLineHandler1(ITest test)
    {
        _test = test;
    }

    public Task ExecuteAsync(CommandLineVerb1 verb)
    {
        _test.DoSomething(verb.InputPath);
        return Task.CompletedTask;
    }
}

public class CommandLineHandler2 : ICommandLineHandler<CommandLineVerb2>
{
    private readonly ITest _test;

    public CommandLineHandler2(ITest test)
    {
        _test = test;
    }

    public Task ExecuteAsync(CommandLineVerb2 verb)
    {
        _test.DoSomething(verb.OutputPath);
        return Task.CompletedTask;
    }
}

public interface ITest
{
    void DoSomething(string message);
}

public class SimpleCommandLineHandler : ICommandLineHandler<SimpleCommandLineOptions>
{
    private readonly ITest _test;

    public SimpleCommandLineHandler(ITest test)
    {
        _test = test;
    }

    public async Task ExecuteAsync(SimpleCommandLineOptions verb)
    {
        _test.DoSomething(verb.FilePath);
    }
}

[TestClass]
public class ExampleTests
{
    [TestMethod]
    public async Task SimpleCommandLineOptions_WithoutHandler_ShouldWork()
    {
        var args = new[]{"-p", "test.txt"};
        IHost host = Host.CreateDefaultBuilder()
            .ConfigureServices(services =>
            {
                services.AddCommandLineCommand<SimpleCommandLineOptions>(args);
            })
            .Build();

        var cliArgs = host.Services.GetRequiredService<CommandLineArguments>();
        cliArgs.Value.Should().BeEquivalentTo(args);
        
        var options = host.Services.GetRequiredService<SimpleCommandLineOptions>();
        options.FilePath.Should().Be("test.txt");

        var handler = host.Services.GetService<ICommandLineHandler<SimpleCommandLineOptions>>();
        handler.Should().BeNull();

        var runner = host.Services.GetService<ICommandLineRunner>();
        runner.Should().BeNull();

        await host.RunCommandLineAsync();
    }
    
    [TestMethod]
    public async Task SimpleCommandLineOptions_WithHandler_ShouldWork()
    {
        var test = new Mock<ITest>();
        var messages = new List<string>();
        test.Setup(x => x.DoSomething(It.IsAny<string>()))
            .Callback((string message) => messages.Add(message));
        
        var args = new[]{"-p", "test.txt"};
        IHost host = Host.CreateDefaultBuilder()
            .ConfigureServices(services =>
            {
                services.AddCommandLineVerbs<CustomVerbBase>();
                services.AddCommandLineCommand<SimpleCommandLineOptions, SimpleCommandLineHandler>(args);
                services.AddSingleton(test.Object);
            })
            .Build();

        var cliArgs = host.Services.GetRequiredService<CommandLineArguments>();
        cliArgs.Value.Should().BeEquivalentTo(args);
        
        var options = host.Services.GetRequiredService<SimpleCommandLineOptions>();
        options.FilePath.Should().Be("test.txt");

        var handler = host.Services.GetService<ICommandLineHandler<SimpleCommandLineOptions>>();
        handler.Should().NotBeNull();

        var runner = host.Services.GetService<ICommandLineRunner>();
        runner.Should().NotBeNull();

        await host.RunCommandLineAsync();

        messages.Count.Should().Be(1);
        messages[0].Should().Be("test.txt");
    }
    
    [TestMethod]
    public async Task CommandLineVerbs_ShouldWork()
    {
        var test = new Mock<ITest>();
        var messages = new List<string>();
        test.Setup(x => x.DoSomething(It.IsAny<string>()))
            .Callback((string message) => messages.Add(message));
        
        var args = new[]{"verb1", "-i", "test.txt"};
        IHost host = Host.CreateDefaultBuilder()
            .ConfigureServices(services =>
            {
                services.AddCommandLineArguments(args);
                services.AddCommandLineVerb<CommandLineVerb1, CommandLineHandler1>();
                services.AddCommandLineVerb<CommandLineVerb2, CommandLineHandler2>();
                services.AddCommandLineVerbs<CustomVerbBase>();
                services.AddSingleton(test.Object);
            })
            .Build();

        var cliArgs = host.Services.GetRequiredService<CommandLineArguments>();
        cliArgs.Value.Should().BeEquivalentTo(args);
        
        var options = host.Services.GetRequiredService<CustomVerbBase>();
        options.FilePath.Should().Be("test.txt");
        (options is CommandLineVerb1).Should().BeTrue();

        var handler1 = host.Services.GetService<ICommandLineHandler<CommandLineVerb1>>();
        handler1.Should().NotBeNull();

        var handler2 = host.Services.GetService<ICommandLineHandler<CommandLineVerb2>>();
        handler2.Should().NotBeNull();

        var runner = host.Services.GetService<ICommandLineRunner>();
        runner.Should().NotBeNull();

        await host.RunCommandLineAsync();

        messages.Count.Should().Be(1);
        messages[0].Should().Be("test.txt");
    }
}