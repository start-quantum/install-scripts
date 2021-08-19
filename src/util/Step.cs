namespace StartQuantum;

public enum InstallStatus
{
    Installed,
    NotInstalled,
    Declined,
    Failed,
    Unknown
}

record Step(
    string Name,
    string Description,
    Action Install,
    Func<InstallStatus>? Check = null,
    List<Step>? DependsOn = null
)
{
    private InstallStatus? installStatus = null;
    public InstallStatus InstallStatus
    {
        get
        {
            if (installStatus == null)
            {
                installStatus = Check?.Invoke() ?? InstallStatus.Unknown;
            }
            return installStatus.Value;
        }
    }

    public InstallStatus Recheck()
    {
        installStatus = null;
        return InstallStatus;
    }

    public void Run()
    {
        // Don't do anything if any dependency was declined.
        if (DependsOn?.Any(step =>
            step.InstallStatus == InstallStatus.Declined ||
            step.InstallStatus == InstallStatus.Failed
        ) ?? false)
        {
            return;
        }

        if (InstallStatus == InstallStatus.Installed)
        {
            Shell.WriteLineInColor($"{Name} is already installed; skipping.\n", ConsoleColor.Blue);
            return;
        }

        foreach (var dependency in DependsOn ?? new List<Step>())
        {
            if (dependency.InstallStatus != InstallStatus.Installed)
            {
                dependency.Run();
                if (dependency.InstallStatus != InstallStatus.Installed)
                {
                    Shell.WriteError($"Dependency {dependency.Name} not installed; skipping {Name}.");
                    Console.WriteLine();
                }
            }
        }

        if (Shell.Prompt($"Install {Name}?\n{Description}"))
        {
            var previousStatus = InstallStatus;
            Install();
            if (previousStatus == InstallStatus.NotInstalled)
            {
                if (Recheck() != InstallStatus.Installed)
                {
                    Shell.WriteError($"{Name} did not seem to install successfully.");
                    installStatus = InstallStatus.Failed;
                    Console.WriteLine();
                }
                else
                {
                    Shell.WriteSuccess($"{Name} installed successfully!\n");
                }
            }
            else
            {
                Console.WriteLine();
            }
        }
        else
        {
            installStatus = InstallStatus.Declined;
            Console.WriteLine();
            return;
        }
    }

};
