#============================================================================
# File:        01_build_reference.ps1
# Project:     ObfuscationStudy pipeline
# Author:      Martin Hrnecek <xhrnecm00>
# Description: Builds reference (non-obfuscated) Release versions of all
#              five test applications. The output assemblies serve as the
#              baseline against which obfuscated builds are compared in
#              subsequent pipeline stages.
#
# Usage:       .\01_build_reference.ps1
# Output:      results/<AppName>/reference/<AppName>.dll
#
# Bachelor's Thesis: Analysis, Methodology and Experimental Evaluation
#                    of IL Code Obfuscation Techniques in .NET Applications
# Year:        2026
#============================================================================

$root = Split-Path $PSScriptRoot -Parent

# List of test applications produced by the project. Each entry maps a
# project name to its .csproj path relative to the repository root.
$projects = @(
    @{ Name = "CliApp";       Path = "src/CliApp/CliApp.csproj" },
    @{ Name = "AlgorithmLib"; Path = "src/AlgorithmLib/AlgorithmLib.csproj" },
    @{ Name = "ClassLibrary"; Path = "src/ClassLibrary/ClassLibrary.csproj" },
    @{ Name = "WebApi";       Path = "src/WebApi/WebApi.csproj" },
    @{ Name = "DesktopApp";   Path = "src/DesktopApp/DesktopApp.csproj" }
)

# Publish each project in Release mode without debug symbols, mirroring
# a typical production deployment scenario.
foreach ($project in $projects) {
    $output = "$root/results/$($project.Name)/reference"
    Write-Host "Building $($project.Name)..." -ForegroundColor Cyan

    dotnet publish "$root/$($project.Path)" -c Release -o $output --nologo -v q

    if ($LASTEXITCODE -eq 0) {
        Write-Host "OK $($project.Name) -> $output" -ForegroundColor Green
    } else {
        Write-Host "FAILED $($project.Name)" -ForegroundColor Red
    }
}

Write-Host "All reference builds done." -ForegroundColor Yellow