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
            System.Diagnostics.Process.Start(
                condaInstaller,
                "/InstallationType=JustMe"
            ).WaitForExit();
        }
        else
        {
            Shell.WriteError("Not yet implemented.");
        }
    },
    Shell.CheckPath("conda")
);
conda.Run();

new Step(
    "Conda PowerShell support",
    "Configures conda for use with PowerShell.",
    () =>
    {
        Console.WriteLine("[MOCK] Running conda init powershell...");
    },
    DependsOn: new() { conda }
).Run();

// TODO: offer to install conda packages in base.

new Step(
    "Conda environment for quantum development",
    "Creates a new conda environment with common packages for quantum development.",
    () =>
    {
        Console.WriteLine("[MOCK] Running conda create...");
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
        Console.WriteLine("[MOCK] Installing dotnet...");
    },
    Check: DotNet.CheckVersion(requiredDotNetVersion)
);
dotnet.Run();


new Step(
    "Quantum project templates",
    "Installs new project templates for quantum libraries and applications.",
    () =>
    {
        Console.WriteLine("[MOCK] Running dotnet new -i...");
    }
).Run();

// VS CODE ///////////////////////////////////////////////////////////////////

var vscode = new Step(
    "VS Code",
    "TODO",
    () =>
    {
        Console.WriteLine("[MOCK] Installing vscode...");
    },
    Shell.CheckPath(VSCode.CommandName)
);
vscode.Run();

new Step(
    "Common extensions for VS Code",
    "TODO",
    () =>
    {
        Console.WriteLine("[MOCK] Installing vscode extensions...");
    },
    Check: 
    Shell
        .CheckPath(VSCode.CommandName)
        .And(VSCode.CheckExtensionsInstalled(
            "ms-python.python",
            "quantum.quantum-devkit-vscode",
            "ms-dotnettools.csharp"
        )),
    DependsOn: new() { vscode }
).Run();
