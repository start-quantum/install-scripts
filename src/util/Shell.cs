namespace StartQuantum;

using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Text;

static class Shell
{
    private static string RunRaw(string cmd, string[] args, bool capture)
    {
        var startInfo = new System.Diagnostics.ProcessStartInfo
        {
            FileName = cmd,
            RedirectStandardOutput = capture,
            RedirectStandardError = capture
        };
        foreach (var arg in args)
        {
            startInfo.ArgumentList.Add(arg);
        }
        var process = Process.Start(startInfo);
        process?.WaitForExit();
        return capture
               ? process?.StandardOutput?.ReadToEnd() ?? ""
               : "";
    }

    public static string Capture(string cmd, params string[] args) =>
        TryGetPath(cmd, out var cmdPath)
        ? RunRaw(cmdPath, args, true)
        : throw new FileNotFoundException($"Command {cmd} not found on PATH.");

    public static void Run(string cmd, params string[] args)
    {
        var _ = TryGetPath(cmd, out var cmdPath)
        ? RunRaw(cmdPath, args, false)
        : throw new FileNotFoundException($"Command {cmd} not found on PATH.");
    }

    [SupportedOSPlatform("windows")]
    static Process? Batch(string[] cmd, bool capture = false)
    {
        var startInfo = new System.Diagnostics.ProcessStartInfo
        {
            FileName = "cmd.exe",
            RedirectStandardOutput = capture
        };
        startInfo.ArgumentList.Add("/C");
        foreach (var arg in cmd)
        {
            startInfo.ArgumentList.Add(arg);
        }
        return Process.Start(startInfo);
    }

    [SupportedOSPlatform("linux")]
    [SupportedOSPlatform("osx")]
    static Process? Bash(string[] cmd, bool capture = false)
    {
        var startInfo = new System.Diagnostics.ProcessStartInfo
        {
            FileName = "bash",
            RedirectStandardOutput = capture
        };
        startInfo.ArgumentList.Add("-c");
        foreach (var arg in cmd)
        {
            startInfo.ArgumentList.Add(arg);
        }
        return Process.Start(startInfo);
    }

    public static bool Prompt(string question, string? detail = null, bool defaultResponse = true)
    {
        Shell.WriteLineInColor(question, ConsoleColor.White);
        if (detail != null)
        {
            Shell.WriteLineInColor(detail, ConsoleColor.Gray);
        }
        Shell.WriteInColor(
            defaultResponse ? "[Y]es (default) / [N]o? " : "[Y]es / [N]o (default)? ",
            ConsoleColor.White
        );

        while (true)
        {
            var response = (System.Console.ReadLine() ?? "").Trim().ToLower();
            if (response == "y" || response == "yes")
            {
                return true;
            }
            else if (response == "n" || response == "no")
            {
                return false;
            }
            else if (response == "")
            {
                return defaultResponse;
            }
            else
            {
                System.Console.WriteLine("Please answer yes or no.");
            }
        }
    }

    public static bool TryGetPath(string cmd, [NotNullWhen(true)] out string? path)
    {
        Process? process = null;
        if (OperatingSystem.IsWindows())
        {
            process = Batch(new[] { "where", cmd, "2>", "nul" }, capture: true);
        }
        else if (OperatingSystem.IsLinux() || OperatingSystem.IsMacOS())
        {
            process = Bash(new[] { $"which {cmd}" }, capture: true);
        }
        else
        {
            throw new Exception("Unsupported on current platform.");
        }

        process?.WaitForExit();
        path = process?.StandardOutput?.ReadToEnd()?.Trim()?.SplitLines()?[0];
        return process?.ExitCode == 0;
    }

    public static bool IsOnPath(string cmd) =>
        TryGetPath(cmd, out var _);

    private static void WithColor(Action action, ConsoleColor color)
    {
        var oldColor = Console.ForegroundColor;
        Console.ForegroundColor = color;
        action();
        Console.ForegroundColor = oldColor;
    }

    public static void WriteLineInColor(string text, ConsoleColor color, bool error = false) =>
        WithColor(() =>
        {
            if (error)
            {
                Console.Error.WriteLine(text);
            }
            else
            {
                Console.WriteLine(text);
            }
        }, color);

    public static void WriteInColor(string text, ConsoleColor color, bool error = false) =>
        WithColor(() =>
        {
            if (error)
            {
                Console.Error.Write(text);
            }
            else
            {
                Console.Write(text);
            }
        }, color);

    public static void WriteError(string text) =>
        WriteLineInColor(text, ConsoleColor.Red, error: true);

    public static void WriteInfo(string text) =>
        WriteLineInColor(text, ConsoleColor.Blue);

    public static void WriteSuccess(string text) =>
        WriteLineInColor(text, ConsoleColor.Green);

    public static void WriteDebug(string text)
    {
        #if DEBUG
            WriteLineInColor($"[DEBUG] {text}", ConsoleColor.DarkGray);
        #endif
    }

    public static Func<InstallStatus> CheckPath(string cmd) =>
        () => IsOnPath(cmd)
              ? InstallStatus.Installed
              : InstallStatus.NotInstalled;

}