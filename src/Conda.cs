using System.Diagnostics.CodeAnalysis;

namespace StartQuantum;

public static class Conda
{
    public static bool TryFind([NotNullWhen(true)] out string? path)
    {
        // Is CONDA_EXE set?
        var envVar = System.Environment.GetEnvironmentVariable("CONDA_EXE");
        if (!string.IsNullOrEmpty(envVar))
        {
            path = envVar;
            return true;
        }

        // Next, check the path to see if someone directly added the conda
        // binary to their path.
        if (Shell.TryGetPath("conda", out path))
        {
            return true;
        }

        // If not, and we're on Windows, shell support may not be enabled.
        // conda should always be a shell function on macOS and Linux, though.
        if (OperatingSystem.IsMacOS())
        {
            path = null;
            return false;
        }

        // We'll try a few common locations, then give up.
        var trialLocations = new[]
        {
            Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Anaconda3"),
            Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Anaconda3"),
            "C:\\Anaconda3"
        };
        var exeLeaf = Path.Join("Scripts", "conda.exe");

        foreach (var trialLocation in trialLocations)
        {
            var candidate = Path.Join(trialLocation, exeLeaf);
            if (File.Exists(candidate))
            {
                path = candidate;
                return true;
            }
        }

        path = null;
        return false;
    }
}