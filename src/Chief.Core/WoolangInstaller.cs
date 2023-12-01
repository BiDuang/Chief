using System.Diagnostics;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Text;
using LibGit2Sharp;
using LibGit2Sharp.Handlers;
using Medallion.Shell;

namespace Chief.Core;

public record WoolangCompilerInfo
{
    public DateTime? BuildTime;
    public string? Commit;
    public required string Path;
    public required string Version;
}

public static class WoolangInstallerExtensions
{
    public static async Task<List<WoolangCompilerInfo>?> GetAllWoolangCompilerInfos()
    {
        var result = new List<WoolangCompilerInfo>();
        string finder, woolang;

        if (OperatingSystem.IsWindows())
        {
            finder = "where.exe";
            woolang = "woodriver.exe";
        }
        else if (OperatingSystem.IsLinux() || OperatingSystem.IsMacOS())
        {
            finder = "which";
            woolang = "woodriver";
        }
        else return null;

        var cmdResult = new List<string>();

        await Command.Run(finder, woolang).RedirectTo(cmdResult).Task;

        var woolangDirs = cmdResult.Where(x => x.Contains(woolang)).ToList();
        foreach (var copDir in woolangDirs)
        {
            cmdResult.Clear();
            try
            {
                await Command.Run(copDir).RedirectTo(cmdResult).Task;
                result.Add(new WoolangCompilerInfo
                {
                    BuildTime = Convert.ToDateTime(cmdResult[3].Replace("Date: ", ""), CultureInfo.InvariantCulture),
                    Commit = cmdResult[2].Replace("Commit: ", "") != "untracked"
                        ? cmdResult[2].Replace("Commit: ", "")
                        : null,
                    Path = copDir,
                    Version = cmdResult[1].Replace("Version: ", "")
                });
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
            }
        }

        return result;
    }
}

public class WoolangInstaller(
    string cachePath,
    ICollection<string> outputPipe,
    VisualStudioInfo? visualStudioInfo = null)
{
    public ProgressHandler? ProgressEvent { get; set; }

    public async Task<bool> BuildWoolangCompiler()
    {
        if (Directory.Exists(cachePath) && Directory.EnumerateFileSystemEntries(cachePath).Any())
            return false;

        var repoGitPath = Repository.Clone("https://git.cinogama.net/cinogamaproject/woolang.git", cachePath,
            new CloneOptions
            {
                BranchName = "release",
                Checkout = true,
                RecurseSubmodules = true,
                OnProgress = ProgressEvent
            });
        var repo = new Repository(repoGitPath);
        await File.WriteAllTextAsync(Path.Combine(cachePath, "src", "wo_info.hpp"), $"\"{repo.Commits.First().Sha}\"");

        Directory.CreateDirectory(Path.Combine(cachePath, "build"));

        var cmd = new Shell(opts => opts
            .WorkingDirectory(Path.Combine(cachePath, "build")));
        if (OperatingSystem.IsWindows())
        {
            if (visualStudioInfo is null) return false;
            if (RuntimeInformation.OSArchitecture is Architecture.X64 or Architecture.X86 or Architecture.Arm64)
                await cmd.Run(Path.Combine(visualStudioInfo.Path,
                            "Common7", "IDE", "CommonExtensions", "Microsoft", "CMake", "CMake", "bin", "cmake.exe"),
                        cachePath, "-DWO_MAKE_OUTPUT_IN_SAME_PATH=ON", "-DCMAKE_BUILD_TYPE=RELWITHDEBINFO")
                    .RedirectTo(outputPipe)
                    .RedirectStandardErrorTo(outputPipe).Task;
            else
                await cmd.Run(Path.Combine(visualStudioInfo.Path,
                            "Common7", "IDE", "CommonExtensions", "Microsoft", "CMake", "CMake", "bin", "cmake.exe"),
                        cachePath, "-DWO_MAKE_OUTPUT_IN_SAME_PATH=ON", "-DCMAKE_BUILD_TYPE=RELWITHDEBINFO",
                        "-DWO_SUPPORT_ASMJIT=OFF") // disable asmjit
                    .RedirectTo(outputPipe)
                    .RedirectStandardErrorTo(outputPipe).Task;

            await cmd.Run(Path.Combine(visualStudioInfo.Path, "MSBuild", "Current", "Bin", "MSBuild.exe"),
                    Path.Combine(cachePath, "build", "driver", "woodriver.vcxproj"), "/p:Configuration=Release",
                    "-maxCpuCount", "-m")
                .RedirectTo(outputPipe)
                .RedirectStandardErrorTo(outputPipe).Task;
        }
        else if (OperatingSystem.IsLinux() || OperatingSystem.IsMacOS())
        {
            if (RuntimeInformation.OSArchitecture is Architecture.X64 or Architecture.X86 or Architecture.Arm64)
                await cmd.Run("cmake", cachePath, "-DWO_MAKE_OUTPUT_IN_SAME_PATH=ON",
                        "-DCMAKE_BUILD_TYPE=RELWITHDEBINFO")
                    .RedirectTo(outputPipe)
                    .RedirectStandardErrorTo(outputPipe).Task;
            else
                await cmd.Run("cmake", cachePath, "-DWO_MAKE_OUTPUT_IN_SAME_PATH=ON",
                        "-DCMAKE_BUILD_TYPE=RELWITHDEBINFO",
                        "-DWO_SUPPORT_ASMJIT=OFF") // disable asmjit
                    .RedirectTo(outputPipe)
                    .RedirectStandardErrorTo(outputPipe).Task;

            await cmd.Run("make", "-C", Path.Combine(cachePath, "build"), "-j")
                .RedirectTo(outputPipe)
                .RedirectStandardErrorTo(outputPipe).Task;
        }

        return true;
    }

    public int InstallWoolangCompiler(string installPath, bool writeEnv = true)
    {
        var woodriver = Path.Combine(cachePath, "build", "Release", "woodriver.exe");
        var libwooExt = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "dll" : "so";
        var libwoo = Path.Combine(cachePath, "build", "Release", $"libwoo.{libwooExt}");
        if (!File.Exists(woodriver) || !File.Exists(libwoo)) return 4;
        Directory.CreateDirectory(installPath);
        File.Copy(woodriver, Path.Combine(installPath, "woodriver.exe"), true);
        File.Copy(libwoo, Path.Combine(installPath, $"libwoo.{libwooExt}"), true);
        if (writeEnv)
        {
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                return 1;
            var envPath = Environment.GetEnvironmentVariable("PATH", EnvironmentVariableTarget.User);
            var sb = new StringBuilder(envPath);
            sb.Append(';');
            sb.Append(installPath);
            Environment.SetEnvironmentVariable("PATH", sb.ToString(), EnvironmentVariableTarget.User);
        }

        return 0;
    }
}