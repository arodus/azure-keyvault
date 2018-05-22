// Copyright Sebastian Karasek, Matthias Koch 2018.
// Distributed under the MIT License.
// https://github.com/nuke-build/azure-keyvault/blob/master/LICENSE

using System;
using System.IO;
using System.Linq;
using Nuke.Common;
using Nuke.Common.Git;
using Nuke.Common.Tools.DotNet;
using Nuke.Common.Tools.GitVersion;
using Nuke.Common.Tools.NuGet;
using Nuke.Common.Utilities;
using Nuke.Common.Utilities.Collections;
using static Nuke.Common.Tools.NuGet.NuGetTasks;
using static Nuke.Common.IO.FileSystemTasks;
using static Nuke.Common.IO.PathConstruction;
using static Nuke.Common.Tools.DotNet.DotNetTasks;
using static Nuke.Common.ChangeLog.ChangelogTasks;
using static Nuke.Common.Tools.Git.GitTasks;

class Build : NukeBuild
{
    [GitVersion] readonly GitVersion GitVersion;
    [GitRepository] readonly GitRepository GitRepository;

    [Parameter("ApiKey for the specified source.")]
    readonly string ApiKey;

    [Parameter("Indicates to push to nuget.org feed.")]
    readonly bool NuGet;

    string Source => NuGet
            ? "https://api.nuget.org/v3/index.json"
            : "https://www.myget.org/F/nukebuild/api/v2/package";

    string SymbolSource => NuGet
            ? "https://nuget.smbsrc.net"
            : "https://www.myget.org/F/nukebuild/symbols/api/v2/package";

    string ChangelogFile => RootDirectory / "CHANGELOG.md";

    public static int Main () => Execute<Build>(x => x.Pack);

    Target Clean => _ => _
            .Executes(() =>
            {
                DeleteDirectories(GlobDirectories(SourceDirectory, "**/bin", "**/obj"));
                EnsureCleanDirectory(OutputDirectory);
            });

    Target Restore => _ => _
            .DependsOn(Clean)
            .Executes(() => DotNetRestore(x => DefaultDotNetRestore));

    Target Compile => _ => _
            .DependsOn(Restore)
            .Executes(() => DotNetBuild(x => DefaultDotNetBuild));

    Target Pack => _ => _
            .DependsOn(Compile)
            .Executes(() =>
            {
                var releaseNotes = ExtractChangelogSectionNotes(ChangelogFile)
                        .Select(x => x.Replace("- ", "\u2022 ").Replace("`", string.Empty).Replace(",", "%2C"))
                        .Concat(string.Empty)
                        .Concat($"Full changelog at {GitRepository.GetGitHubBrowseUrl(ChangelogFile)}")
                        .JoinNewLine();

                DotNetPack(x => DefaultDotNetPack.SetPackageReleaseNotes(releaseNotes));
            });

    Target Changelog => _ => _
            .OnlyWhen(() => InvokedTargets.Contains(nameof(Changelog)))
            .Executes(() =>
            {
                FinalizeChangelog(ChangelogFile, GitVersion.SemVer, GitRepository);

                Git($"add {ChangelogFile}");
                Git($"commit -m \"Finalize {Path.GetFileName(ChangelogFile)} for {GitVersion.SemVer}.\" -m \"+semver: skip\"");
                Git($"tag -f {GitVersion.SemVer}");
            });

    Target Push => _ => _
            .DependsOn(Pack)
            .Requires(() => ApiKey)
            .Requires(() => !GitHasUncommitedChanges())
            .Requires(() => !NuGet || GitVersionAttribute.Bump.HasValue)
            .Requires(() => !NuGet || Configuration.EqualsOrdinalIgnoreCase("release"))
            .Requires(() => !NuGet || GitVersion.BranchName.Equals("master"))
            .Executes(() => GlobFiles(OutputDirectory, "*.nupkg")
                    .Where(x => !x.EndsWith("symbols.nupkg"))
                    .ForEach(x => NuGetPush(s => s
                            .SetTargetPath(x)
                            .SetSource(Source)
                            .SetSymbolSource(SymbolSource)
                            .SetApiKey(ApiKey))));
}
