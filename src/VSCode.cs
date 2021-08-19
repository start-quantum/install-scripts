namespace StartQuantum;

public static class VSCode
{
    public static readonly string CommandName =
        OperatingSystem.IsWindows()
        ? "code.cmd"
        : "code";

    public static IEnumerable<string> GetInstalledExtensions() =>
        Shell
        .Capture(CommandName, "--list-extensions")
        .SplitLines()
        .Select(line => line.Trim())
        .ToHashSet();

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