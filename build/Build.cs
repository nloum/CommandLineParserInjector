using System.Linq;
using Nuke.Common;
using Nuke.Common.CI;
using Nuke.Common.Execution;
using Nuke.Common.Git;
using Nuke.Common.IO;
using Nuke.Common.ProjectModel;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.DotNet;
using Nuke.Common.Tools.GitVersion;
using static Nuke.Common.IO.FileSystemTasks;
using static Nuke.Common.Tools.DotNet.DotNetTasks;

[CheckBuildProjectConfigurations]
[ShutdownDotNetAfterServerBuild]
class Build : NukeBuild
{
    /// Support plugins are available for:
    ///   - JetBrains ReSharper        https://nuke.build/resharper
    ///   - JetBrains Rider            https://nuke.build/rider
    ///   - Microsoft VisualStudio     https://nuke.build/visualstudio
    ///   - Microsoft VSCode           https://nuke.build/vscode

    public static int Main () => Execute<Build>(x => x.Compile);

    [Parameter("Configuration to build - Default is 'Debug' (local) or 'Release' (server)")]
    readonly Configuration Configuration = /*IsLocalBuild ? Configuration.Debug : */Configuration.Release;

    [Solution] readonly Solution Solution;
    [GitRepository] readonly GitRepository GitRepository;
    [GitVersion(Framework = "net5.0")] readonly GitVersion GitVersion;
    [Parameter("NuGet server URL.")]
    readonly string NugetSource = "https://api.nuget.org/v3/index.json";
    [Parameter("API Key for the NuGet server.")]
    readonly string NugetApiKey;

    AbsolutePath OutputDirectory => RootDirectory / "output";
    
    Project[] PackageProjects => new[]
    {
        Solution.GetProject("CommandLineParserInjector"),
    };

    Project TemplateProject => Solution.GetProject("templatepack");

    Target Clean => _ => _
        .Before(Restore)
        .Executes(() =>
        {
            EnsureCleanDirectory(OutputDirectory);

            DotNetClean(_ => _
                .SetConfiguration(Configuration)
                .SetProject(Solution));
        });

    Target Restore => _ => _
        .Executes(() =>
        {
            DotNetRestore(s => s
                .SetProjectFile(Solution));
        });

    Target Compile => _ => _
        .DependsOn(Restore)
        .Executes(() =>
        {
            DotNetBuild(s => s
                .SetProjectFile(Solution)
                .SetConfiguration(Configuration)
                .SetAssemblyVersion(GitVersion.AssemblySemVer)
                .SetFileVersion(GitVersion.AssemblySemFileVer)
                .SetInformationalVersion(GitVersion.InformationalVersion)
                .EnableNoRestore());
        });

    Target Pack => _ => _
        .DependsOn(Compile)
        .Requires(() => Configuration == Configuration.Release)
        .Executes(() =>
        {
            DotNetPack(s => s
                .EnableNoRestore()
                .EnableNoBuild()
                .SetConfiguration(Configuration)
                .SetOutputDirectory(OutputDirectory)
                .SetVersion(GitVersion.NuGetVersionV2)
                .SetIncludeSymbols(true)
                .SetSymbolPackageFormat(DotNetSymbolPackageFormat.snupkg)
                .AddProperty("ContinuousIntegrationBuild", "true")
                .CombineWith(
                    from project in PackageProjects
                    select new { project }, (cs, v) => cs
                        .SetProject(v.project))
            );

            // TODO - find out how to make the template depend on the latest version of the library
            // var originalTemplateProjectContents = File.ReadAllText(RootDirectory / "working/templates/CommandLineTemplate/CommandLineTemplate.csproj");
            // var regex = new Regex("<ProjectReference.+/>");
            // var newTemplateProjectContents = regex.Replace(originalTemplateProjectContents,
            //     $"<PackageReference Include=\"CommandLineParserInjector\" Version=\"{GitVersion.NuGetVersionV2}\" />");
            // if (newTemplateProjectContents == originalTemplateProjectContents)
            // {
            //     throw new InvalidOperationException(
            //         $"The project reference was not successfully replaced with a package reference: \"{originalTemplateProjectContents}\"");
            // }
            // File.WriteAllText(TemplateProject.Path, newTemplateProjectContents);
            
            DotNetPack(s => s
                .SetVersion(GitVersion.NuGetVersionV2)
                //.EnableNoRestore()
                .SetOutputDirectory(OutputDirectory)
                .CombineWith(
                    from project in new[]{TemplateProject}
                    select new { project }, (cs, v) => cs
                        .SetProject(v.project))
            );
        });

    Target Push => _ => _
        .DependsOn(Pack)
        .Consumes(Pack)
        .Requires(() => Configuration == Configuration.Release)
        .Executes(() =>
        {
            DotNetNuGetPush(s => s
                .SetSource(NugetSource)
                .SetApiKey(NugetApiKey)
                .SetSkipDuplicate(true)
                .CombineWith(OutputDirectory.GlobFiles("*.nupkg"), (s, v) => s
                    .SetTargetPath(v)
                )
            );
        });
}
