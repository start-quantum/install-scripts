using System.Diagnostics.CodeAnalysis;

namespace StartQuantum;

public static class DotNet
{
    public static bool TryFind([NotNullWhen(true)] out string? path)
    {
        if (Shell.TryGetPath("dotnet", out path))
        {
            return true;
        }

        // If we just installed dotnet, it may not be on the PATH yet.
        // Let's check where it's installed by default in different OSes.
        if (OperatingSystem.IsWindows())
        {
            var candidate = Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "dotnet", "dotnet.exe");
            if (File.Exists(candidate))
            {
                path = candidate;
                return true;
            }
        }
        else
        {
            Shell.WriteError("Finding dotnet outside of $PATH is not yet implemented on Linux and macOS.");
        }

        path = null;
        return false;
    }

    public static Func<InstallStatus> CheckVersion(string version) => () =>
    {
        if (!TryFind(out var path))
        {
            return InstallStatus.NotInstalled;
        }

        var process = System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
        {
            FileName = path,
            Arguments = "--list-sdks",
            RedirectStandardOutput = true
        });
        process?.WaitForExit();
        var sdks = process?.StandardOutput?.ReadToEnd() ?? "";
        foreach (var sdk in sdks.Split(Environment.NewLine))
        {
            if (sdk.StartsWith($"{version}."))
            {
                return InstallStatus.Installed;
            }
        }
        return InstallStatus.NotInstalled;
    };
}
