using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using Microsoft.VisualStudio.Setup.Configuration;

namespace ChiefCore;

public static partial class WoolangInstaller
{
    [GeneratedRegex(@"\x1B\[[0-9;]*[mK]")]
    private static partial Regex RemoveColorCharsRegex();

    public static List<VisualStudioInfo> GetAllVisualStudioInfos()
    {
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

    public static async Task<List<WoolangCompilerInfo>> GetAllWoolangCompilerInfos()
    {
        var result = new List<WoolangCompilerInfo>();
        var finder = string.Empty;
        var woolang = string.Empty;
        switch (Environment.OSVersion.Platform)
        {
            case PlatformID.Win32NT:
                finder = "where.exe";
                woolang = "woodriver.exe";
                break;
            case PlatformID.Unix:
                finder = "which";
                woolang = "woodriver";
                break;
            case PlatformID.Win32S:
            case PlatformID.Win32Windows:
            case PlatformID.WinCE:
            case PlatformID.Xbox:
            case PlatformID.MacOSX:
            case PlatformID.Other:
            default:
                throw new PlatformNotSupportedException("This Platform is not supported.");
        }

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
            process = Process.Start(new ProcessStartInfo
            {
                FileName = copDir,
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            });
            var woolangInfo = (await process!.StandardOutput.ReadToEndAsync()).Split("\r\n").ToList();
        }

        return result;
    }

    public class VisualStudioInfo
    {
        public string Path = string.Empty;
        public InstanceState State = InstanceState.None;
        public string Version = string.Empty;
    }

    public class WoolangCompilerInfo
    {
        public string? BuildTime = null;
        public string? Commit = null;
        public string Path = string.Empty;
        public string Version = string.Empty;
    }
}