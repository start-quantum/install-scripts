param(
    [string]
    $Configuration = "Release"
);

Push-Location (Join-Path $PSScriptRoot "src")
    dotnet publish -c $Configuration --self-contained -r win10-x64
    dotnet publish -c $Configuration --self-contained -r osx-x64
    dotnet publish -c $Configuration --self-contained -r linux-x64
Pop-Location
