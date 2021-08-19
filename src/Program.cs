using StartQuantum;

// CONDA /////////////////////////////////////////////////////////////////////

var conda = new Step(
    "Conda",
    "Installs a scientific distribution of Python.",
    () =>
    {
        if (OperatingSystem.IsWindows())
        {
            var condaInstaller = Download.File(
                "https://repo.anaconda.com/archive/Anaconda3-2021.05-Windows-x86_64.exe",
                "Anaconda3-2021.05-Windows-x86_64.exe"
            ).Result;
            Shell.WriteLineInColor("Installing conda, please wait...", ConsoleColor.White);
            System.Diagnostics.Process.Start(
                condaInstaller,
                "/InstallationType=JustMe /S"
            ).WaitForExit();
        }
        else
        {
            Shell.WriteError("Not yet implemented.");
        }
    },
    Check: () =>
    {
        Shell.WriteDebug("Checking if conda is installed.");
        if (Conda.TryFind(out var path))
        {
            Shell.WriteDebug($"Found conda at '{path}'.");
            return InstallStatus.Installed;
        }
        else
        {
            // On Windows, it may be somewhere weird that's not on PATH.
            return OperatingSystem.IsWindows()
                   ? InstallStatus.Unknown
                   : InstallStatus.NotInstalled;
        }
    }
);
conda.Run();

new Step(
    "Conda command-line support",
    "Configures conda for use with common command line shells, including bash and PowerShell.",
    () =>
    {
        if (Conda.TryFind(out var path))
        {
            System.Diagnostics.Process.Start(path, "init --all").WaitForExit();
        }
        else
        {
            Shell.WriteError("Could not find conda to configure for command-line use.");
            return;
        }

        // On Windows, we may need to set the execution policy.
        if (OperatingSystem.IsWindows())
        {
            Console.WriteLine(
                "By default, the security policy for PowerShell on Windows prevents adding conda to your profile. " +
                "If you select \"yes\" here, this installer will set the security policy to allow conda. " +
                "See https://docs.microsoft.com/powershell/module/microsoft.powershell.core/about/about_execution_policies?view=powershell-7.1"
            );
            if (Shell.Prompt("Set PowerShell execution policy to allow using conda?"))
            {
                Shell.Run("powershell", "-NoProfile", "-Command", "Set-ExecutionPolicy", "-Scope", "CurrentUser", "RemoteSigned");
                if (Shell.IsOnPath("pwsh"))
                {
                    Shell.Run("pwsh", "-NoProfile", "-Command", "Set-ExecutionPolicy", "-Scope", "CurrentUser", "RemoteSigned");
                }
            }
        }
    },
    DependsOn: new() { conda }
).Run();

// TODO: Re-enable once https://github.com/microsoft/iqsharp/issues/500 is fixed.
// new Step(
//     "quantum packages in base conda environment",
//     "Installs common quantum packages in your base environment, making them available by default.",
//     () =>
//     {
//         if (Conda.TryFind(out var path))
//         {
//             System.Diagnostics.Process.Start(
//                 path,
//                 "install --yes -n base -c conda-forge -c quantum-engineering qutip matplotlib qsharp notebook"
//             )
//             .WaitForExit();
//         }
//         else
//         {
//             Shell.WriteError("Could not find conda to install packages into base.");
//             return;
//         }
//     },
//     DependsOn: new() { conda }
// ).Run();

new Step(
    "Conda environment for quantum development",
    "Creates a new conda environment with common packages for quantum development.",
    () =>
    {
        if (Conda.TryFind(out var path))
        {
            System.Diagnostics.Process.Start(
                path,
                "create --yes -n quantum -c conda-forge -c quantum-engineering qutip matplotlib qsharp numpy scipy notebook"
            )
            .WaitForExit();
            Shell.WriteLineInColor("New conda environment created. To use, run `conda activate quantum`.", ConsoleColor.White);
        }
        else
        {
            Shell.WriteError("Could not find conda to install packages into new environment.");
            return;
        }
    },
    DependsOn: new() { conda }
).Run();

// .NET //////////////////////////////////////////////////////////////////////

const string requiredDotNetVersion = "3.1";
// NB: Replace .NET Core SDK by .NET SDK with 6.0.
var dotnet = new Step(
    $".NET Core SDK {requiredDotNetVersion}",
    "Installs a cross-platform toolchain for writing classical and quantum applications.",
    () =>
    {
        if (OperatingSystem.IsWindows())
        {
            var dotNetInstaller = Download.File(
                "https://download.visualstudio.microsoft.com/download/pr/046165a4-10d4-4156-8e65-1d7b2cbd304e/a4c7b01f6bf7199669a45ab6a03803ac/dotnet-sdk-3.1.412-win-x64.exe",
                "dotnet-sdk-3.1.412-win-x64.exe"
            ).Result;
            Shell.WriteLineInColor($"Installing .NET Core SDK {requiredDotNetVersion}...", ConsoleColor.White);
            System.Diagnostics.Process.Start(
                dotNetInstaller,
                "/install /quiet /norestart"
            ).WaitForExit();
        }
        else
        {
            Shell.WriteError("Not yet implemented.");
        }
    },
    Check: DotNet.CheckVersion(requiredDotNetVersion)
);
dotnet.Run();

new Step(
    "Quantum project templates",
    "Installs new project templates for quantum libraries and applications.",
    () =>
    {
        if (DotNet.TryFind(out var path))
        {
            System.Diagnostics.Process.Start(
                path,
                "new -i Microsoft.Quantum.ProjectTemplates"
            )
            .WaitForExit();
        }
        else
        {
            Shell.WriteError("Could not find dotnet to install new project templates.");
            return;
        }
    },
    Check: () =>
    {
        // TODO: check if already installed here.
        return InstallStatus.Unknown;
    },
    DependsOn: new() { dotnet }
).Run();

// VS CODE ///////////////////////////////////////////////////////////////////

var vscode = new Step(
    "VS Code",
    "Installs a cross-platform development environment for use with many different languages, including Python, Q#, and JavaScript.",
    () =>
    {
        if (OperatingSystem.IsWindows())
        {
            var vscodeInstaller = Download.File(
                "https://code.visualstudio.com/sha/download?build=stable&os=win32-x64-user",
                "vscode-user-setup.exe"
            ).Result;
            Shell.WriteLineInColor($"Installing VS Code...", ConsoleColor.White);
            System.Diagnostics.Process.Start(
                vscodeInstaller,
                "/verysilent /norestart /mergetasks=!runcode"
            ).WaitForExit();
        }
        else
        {
            Shell.WriteError("Not yet implemented.");
        }
    },
    Check: () =>
    {
        Shell.WriteDebug("Checking if code is installed.");
        if (VSCode.TryFind(out var path))
        {
            Shell.WriteDebug($"Found code at '{path}'.");
            return InstallStatus.Installed;
        }
        else
        {
            return InstallStatus.NotInstalled;
        }
    }
);
vscode.Run();

var vscodeExtensions = new[]
{
    "ms-python.python",
    "quantum.quantum-devkit-vscode",
    "ms-dotnettools.csharp"
};

new Step(
    "Common extensions for VS Code",
    "Installs VS Code extensions for working with Python, Q#, and C#.",
    () =>
    {
        if (VSCode.TryFind(out var path))
        {
            foreach (var extId in vscodeExtensions)
            {
                Console.WriteLine($"Installing extension {extId}...");
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = path,
                    Arguments = $"--install-extension {extId}",
                    RedirectStandardError = true
                })
                ?.WaitForExit();
            }
        }
        else
        {
            Shell.WriteError("Could not find code to install extensions.");
            return;
        }
    },
    Check: vscode
           .Check!
           .And(VSCode.CheckExtensionsInstalled(
               vscodeExtensions
           )),
    DependsOn: new() { vscode }
).Run();

System.Console.WriteLine("Installation completed! The following installation steps were performed:");
foreach (var step in Step.CompletedSteps)
{
    System.Console.WriteLine($"- {step.Name}");
}

if (OperatingSystem.IsWindows())
{
    System.Console.WriteLine("Press any key to continue...");
    System.Console.ReadKey();
}
