#============================================================================
# File:        02_obfuscate.ps1
# Project:     ObfuscationStudy pipeline
# Author:      Martin Hrnecek <xhrnecm00>
# Description: Applies the selected obfuscator to the reference builds of
#              all test applications. Currently supports the Obfuscar
#              open-source obfuscator; Eazfuscator.NET and .NET Reactor
#              are invoked from the master script (run_all.ps1) which
#              also performs prerequisite checks for those commercial
#              tools.
#
# Usage:       .\02_obfuscate.ps1 [obfuscator]
#              obfuscator: "obfuscar" (default)
#
# Bachelor's Thesis: Analysis, Methodology and Experimental Evaluation
#                    of IL Code Obfuscation Techniques in .NET Applications
# Year:        2026
#============================================================================

$root = Split-Path $PSScriptRoot -Parent
$configTemplate = "$root/tools/obfuscar/obfuscar.xml"

# Test applications and their primary assembly names.
$projects = @(
    @{ Name = "CliApp";       Dll = "CliApp.dll" },
    @{ Name = "AlgorithmLib"; Dll = "AlgorithmLib.dll" },
    @{ Name = "ClassLibrary"; Dll = "ClassLibrary.dll" },
    @{ Name = "WebApi";       Dll = "WebApi.dll" },
    @{ Name = "DesktopApp";   Dll = "DesktopApp.dll" }
)

# Resolve the obfuscator name from the first command-line argument.
# Defaults to "obfuscar" when no argument is provided.
$obfuscator = $args[0]
if (-not $obfuscator) { $obfuscator = "obfuscar" }

Write-Host "Obfuscating with: $obfuscator" -ForegroundColor Cyan

foreach ($project in $projects) {
    $refPath = "$root/results/$($project.Name)/reference"
    $outPath = "$root/results/$($project.Name)/obfuscated_$obfuscator"

    if (-not (Test-Path $refPath)) {
        Write-Host "SKIP $($project.Name) - reference not found" -ForegroundColor Yellow
        continue
    }

    New-Item $outPath -ItemType Directory -Force | Out-Null

    if ($obfuscator -eq "obfuscar") {
        # Generate a per-application Obfuscar config file by substituting
        # the input/output paths and module name into the template, then
        # invoke the Obfuscar CLI installed via NuGet.
        $config = (Get-Content $configTemplate) -replace "PLACEHOLDER", $project.Dll.Replace(".dll", "")
        $tempConfig = "$root/results/$($project.Name)/obfuscar_config.xml"
        $config -replace '\./obfuscated', $outPath -replace '\.', $refPath | Set-Content $tempConfig

        $configContent = @"
<?xml version="1.0" encoding="UTF-8"?>
<Obfuscator>
  <Var name="InPath" value="$refPath" />
  <Var name="OutPath" value="$outPath" />
  <Var name="KeepPublicApi" value="false" />
  <Var name="HidePrivateApi" value="true" />
  <Var name="RenameFields" value="true" />
  <Var name="RenameProperties" value="true" />
  <Var name="RenameEvents" value="true" />
  <Var name="ObfuscateAssembly" value="true" />
  <Module file="$refPath/$($project.Dll)" />
</Obfuscator>
"@
        $configContent | Set-Content $tempConfig -Encoding UTF8

        Write-Host "Obfuscating $($project.Name)..." -ForegroundColor Cyan
        obfuscar.console $tempConfig

        if ($LASTEXITCODE -eq 0) {
            Write-Host "OK $($project.Name)" -ForegroundColor Green
        } else {
            Write-Host "FAILED $($project.Name)" -ForegroundColor Red
        }
    }
}

Write-Host "Done." -ForegroundColor Yellow