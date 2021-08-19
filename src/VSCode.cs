using System.Diagnostics.CodeAnalysis;

namespace StartQuantum;

public static class VSCode
{
    public static readonly string CommandName =
        OperatingSystem.IsWindows()
        ? "code.cmd"
        : "code";

    public static bool TryFind([NotNullWhen(true)] out string? path)
    {
        if (Shell.TryGetPath(CommandName, out path))
        {
            return true;
        }

        // If we just installed dotnet, it may not be on the PATH yet.
        // Let's check where it's installed by default in different OSes.
        if (OperatingSystem.IsWindows())
        {
            var candidate = Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Programs", "Microsoft VS Code", "bin", CommandName);
            if (File.Exists(candidate))
            {
                path = candidate;
                return true;
            }
        }
        else
        {
            Shell.WriteError("Finding code outside of $PATH is not yet implemented on Linux and macOS.");
        }

        path = null;
        return false;
    }

    public static IEnumerable<string> GetInstalledExtensions()
    {
        if (!TryFind(out var path))
        {
            throw new FileNotFoundException($"Could not find code to list extensions.");
        }

        var process = System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
        {
            FileName = path,
            Arguments = "--list-extensions",
            RedirectStandardOutput = true
        });
        process?.WaitForExit();
        return process
            ?.StandardOutput
            ?.ReadToEnd()
            ?.SplitLines()
            ?.Select(line => line.Trim())
            ?.ToHashSet()
            ?? new HashSet<string>();
    }

    public static Func<InstallStatus> CheckExtensionsInstalled(
        params string[] extensionIds
    ) => () =>
        {
            // TODO: check for vs code itself.
            var installedExtensions = GetInstalledExtensions();
            foreach (var extId in extensionIds)
            {
                if (!installedExtensions.Contains(extId))
                {
                    System.Console.WriteLine($"ext {extId} was not installed.");
                    return InstallStatus.NotInstalled;
                }
            }
            return InstallStatus.Installed;
        };
}