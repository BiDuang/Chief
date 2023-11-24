using System.Diagnostics;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using LibGit2Sharp;
using LibGit2Sharp.Handlers;
using Medallion.Shell;
using Microsoft.VisualStudio.Setup.Configuration;

namespace ChiefCore;

public record VisualStudioInfo
{
    public required string Path;
    public InstanceState State = InstanceState.None;
    public required string Version;
}

public record WoolangCompilerInfo
{
    public DateTime? BuildTime;
    public string? Commit;
    public required string Path;
    public required string Version;
}

public static partial class WoolangInstallerExtensions
{
    [GeneratedRegex(@"\x1B\[[0-9;]*[mK]")]
    private static partial Regex RemoveColorCharsRegex();

    public static List<VisualStudioInfo>? GetAllVisualStudioInfos()
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            return null;

        var result = new List<VisualStudioInfo>();
        try
        {
            var query = new SetupConfiguration();
            var query2 = (ISetupConfiguration2)query;
            var e = query2.EnumAllInstances();
            int fetched;
            var instances = new ISetupInstance[1];
            do
            {
                e.Next(1, instances, out fetched);
                if (fetched <= 0) continue;
                var instance = (ISetupInstance2)instances[0];
                var state = instance.GetState();
                result.Add(new VisualStudioInfo
                {
                    State = state,
                    Version = instance.GetInstallationVersion(),
                    Path = instance.GetInstallationPath()
                });
            } while (fetched > 0);
        }
        catch (COMException ex) when (ex.HResult == unchecked((int)0x80040154))
        {
            Debug.WriteLine("The query API is not registered. Assuming no instances are installed.");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error 0x{ex.HResult:x8}: {ex.Message}");
        }

        return result;
    }

    public static async Task<List<WoolangCompilerInfo>?> GetAllWoolangCompilerInfos()
    {
        var result = new List<WoolangCompilerInfo>();
        string finder, woolang;

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            finder = "where";
            woolang = "woodriver.exe";
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux) || RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            finder = "which";
            woolang = "woodriver";
        }
        else return null;


        var process = Process.Start(new ProcessStartInfo
        {
            FileName = finder,
            Arguments = woolang,
            RedirectStandardOutput = true,
            UseShellExecute = false,
            CreateNoWindow = true
        });
        var woolangDirs = (await process!.StandardOutput.ReadToEndAsync()).Split("\r\n").ToList();
        woolangDirs.RemoveAll(x => !x.EndsWith(woolang));
        foreach (var copDir in woolangDirs)
        {
            try
            {
                process = Process.Start(new ProcessStartInfo
                {
                    FileName = copDir,
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                });
                var woolangInfo = RemoveColorCharsRegex().Replace(await process!.StandardOutput.ReadToEndAsync(), "")
                    .Split("\r\n").ToList();
                result.Add(new WoolangCompilerInfo
                {
                    BuildTime = Convert.ToDateTime(woolangInfo[3].Remove(0, 6), CultureInfo.InvariantCulture),
                    Commit = woolangInfo[2].Remove(0, 8) != "untracked" ? woolangInfo[2].Remove(0, 8) : null,
                    Path = copDir,
                    Version = woolangInfo[1].Remove(0, 9)
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

public class WoolangInstaller(string cachePath, ICollection<string> outputPipe,
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
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
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
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux) || RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
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