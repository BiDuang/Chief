using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Security.Principal;
using Medallion.Shell;
using Microsoft.VisualStudio.Setup.Configuration;

namespace Chief.Core;

public record VisualStudioInfo
{
    public required string Path;
    public InstanceState State = InstanceState.None;
    public required string Version;
}

public static class Dependencies
{
    public static async Task<bool> IsAdminPermission()
    {
        if (OperatingSystem.IsWindows())
        {
            var user = WindowsIdentity.GetCurrent();
            var principal = new WindowsPrincipal(user);
            return principal.IsInRole(WindowsBuiltInRole.Administrator);
        }

        if (!OperatingSystem.IsLinux() && !OperatingSystem.IsMacOS())
            throw new PlatformNotSupportedException("Chief does not support this platform.");

        var result = new List<string>();
        await Command.Run("whoami").RedirectTo(result).Task;
        return result[0] == "root";
    }

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
}