namespace StartQuantum;

public static class DotNet
{
    public static Func<InstallStatus> CheckVersion(string version) => () =>
    {
        if (!Shell.IsOnPath("dotnet"))
        {
            return InstallStatus.NotInstalled;
        }

        var sdks = Shell.Capture("dotnet", "--list-sdks");
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
