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
    public static List<Step> CompletedSteps = new ();

    private InstallStatus? installStatus = null;
    public InstallStatus InstallStatus
    {
        get
        {
            if (installStatus == null)
            {
                try
                {
                    installStatus = Check?.Invoke() ?? InstallStatus.Unknown;
                }
                catch (Exception ex)
                {
                    Shell.WriteError($"Exception while checking if {Name} is installed:\n{ex}");
                    installStatus = InstallStatus.Unknown;
                }
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
                    return;
                }
            }
        }

        if (Shell.Prompt($"Install {Name}?", Description))
        {
            var previousStatus = InstallStatus;
            try
            {
                Install();
            }
            catch (Exception ex)
            {
                installStatus = InstallStatus.Failed;
                Shell.WriteError($"Exception running install step {Name}:\n{ex}");
            }
            Shell.WriteDebug($"Finished installing {Name}, now checking if install was successful.");
            Recheck();

            if (previousStatus == InstallStatus.NotInstalled)
            {
                if (InstallStatus != InstallStatus.Installed)
                {
                    Shell.WriteError($"{Name} did not seem to install successfully.");
                    installStatus = InstallStatus.Failed;
                    Console.WriteLine();
                }
                else
                {
                    Shell.WriteSuccess($"{Name} installed successfully!\n");
                    Step.CompletedSteps.Add(this);
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
