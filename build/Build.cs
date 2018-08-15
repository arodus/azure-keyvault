// Copyright Sebastian Karasek, Matthias Koch 2018.
// Distributed under the MIT License.
// https://github.com/nuke-build/azure-keyvault/blob/master/LICENSE

using System;
using System.Linq;
using Nuke.Common;
using Nuke.Common.Git;
using Nuke.Common.Tools.DotNet;
using Nuke.Common.Tools.GitVersion;
using Nuke.Common.Utilities;
using Nuke.Common.Utilities.Collections;
using Nuke.GitHub;
using static Nuke.Common.IO.FileSystemTasks;
using static Nuke.Common.IO.PathConstruction;
using static Nuke.Common.Tools.DotNet.DotNetTasks;
using static Nuke.Common.ChangeLog.ChangelogTasks;
using static Nuke.Common.Tools.Git.GitTasks;
using static Nuke.GitHub.GitHubTasks;

class Build : NukeBuild
{
    const string c_toolNamespace = "Nuke.Azure.KeyVault";
    const string c_addonRepoOwner = "nuke-build";
    const string c_addonRepoName = "azure";
    const string c_addonName = "Azure CLI";

    [GitVersion] readonly GitVersion GitVersion;
    [GitRepository] readonly GitRepository GitRepository;

    [Parameter("Api key to push packages to NuGet.org.")] readonly string NuGetApiKey;
    [Parameter("Api key to access the GitHub.")] readonly string GitHubApiKey;

    string ChangelogFile => RootDirectory / "CHANGELOG.md";
    bool IsReleaseBranch => GitRepository.Branch.NotNull().StartsWith("release/");
    bool IsMasterBranch => GitRepository.Branch == "master";

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
            .OnlyWhen(ShouldUpdateChangelog)
            .Executes(() =>
            {
                FinalizeChangelog(ChangelogFile, GitVersion.MajorMinorPatch, GitRepository);
            });

    Target Push => _ => _
            .DependsOn(Pack)
            .Requires(() => NuGetApiKey)
            .Requires(() => GitHasCleanWorkingCopy())
            .Requires(() => Configuration.EqualsOrdinalIgnoreCase("release"))
            .Requires(() => IsReleaseBranch || IsMasterBranch)
            .Executes(() => GlobFiles(OutputDirectory, "*.nupkg")
                    .Where(x => !x.EndsWith("symbols.nupkg"))
                    .NotEmpty()
                    .ForEach(x => DotNetNuGetPush(s => s
                            .SetTargetPath(x)
                            .SetSource("https://api.nuget.org/v3/index.json")
                            .SetSymbolSource("https://nuget.smbsrc.net")
                            .SetApiKey(NuGetApiKey))));

    Target Release => _ => _
            .Requires(() => GitHubApiKey)
            .DependsOn(Push)
            .After(PrepareRelease)
            .Executes(async () =>
            {
                var releaseNotes = new[]
                                   {
                                           $"- [Nuget](https://www.nuget.org/packages/{c_toolNamespace}/{GitVersion.SemVer})",
                                           $"- [Changelog](https://github.com/{c_addonRepoOwner}/{c_addonRepoName}/blob/{GitVersion.SemVer}/CHANGELOG.md)"
                                   };

                await PublishRelease(x => x.SetToken(GitHubApiKey)
                        .SetArtifactPaths(GlobFiles(OutputDirectory, "*.nupkg").ToArray())
                        .SetRepositoryName(c_addonRepoName)
                        .SetRepositoryOwner(c_addonRepoOwner)
                        .SetCommitSha("master")
                        .SetName($"NUKE {c_addonName} Addon v{GitVersion.MajorMinorPatch}")
                        .SetTag($"{GitVersion.MajorMinorPatch}")
                        .SetReleaseNotes(releaseNotes.Join("\n"))
                );
            });

    Target PrepareRelease => _ => _
            .Before(Restore)
            .DependsOn(Changelog, Clean)
            .Executes(() =>
            {
                var releaseBranch = IsReleaseBranch ? GitRepository.Branch : $"release/v{GitVersion.MajorMinorPatch}";
                var isMasterBranch = IsMasterBranch;
                var pushMaster = false;
                if (!isMasterBranch && !IsReleaseBranch) Git($"checkout -b {releaseBranch}");

                if (!GitHasCleanWorkingCopy())
                {
                    Git($"add {ChangelogFile}");
                    Git($"commit -m \"Finalize v{GitVersion.MajorMinorPatch}\"");
                    pushMaster = true;
                }

                if (!isMasterBranch)
                {
                    Git("checkout master");
                    Git($"merge --no-ff --no-edit {releaseBranch}");
                    Git($"branch -D {releaseBranch}");
                    pushMaster = true;
                }

                if (IsReleaseBranch) Git($"push origin --delete {releaseBranch}");
                if (pushMaster) Git("push origin master");
            });

    bool ShouldUpdateChangelog ()
    {
        bool TryGetChangelogSectionNotes (string tag, out string[] sectionNotes)
        {
            sectionNotes = new string[0];
            try
            {
                sectionNotes = ExtractChangelogSectionNotes(ChangelogFile, tag).ToArray();
                return sectionNotes.Length > 0;
            }
            catch (Exception)
            {
                return false;
            }
        }

        var nextSectionAvailable = TryGetChangelogSectionNotes("vNext", out var vNextSection);
        var semVerSectionAvailable = TryGetChangelogSectionNotes(GitVersion.MajorMinorPatch, out var semVerSection);
        if (semVerSectionAvailable)
        {
            ControlFlow.Assert(!nextSectionAvailable, $"{GitVersion.MajorMinorPatch} is already in changelog.");
            return false;
        }

        return nextSectionAvailable;
    }
}
