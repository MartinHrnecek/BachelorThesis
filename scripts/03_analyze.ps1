#============================================================================
# File:        03_analyze.ps1
# Project:     ObfuscationStudy pipeline
# Author:      Martin Hrnecek <xhrnecm00>
# Description: Runs the Analyzer tool on the specified build variant of
#              every test application and persists the resulting metrics
#              as JSON files. Supports either the reference build or any
#              of the obfuscated variants.
#
# Usage:       .\03_analyze.ps1 [version]
#              version: "reference" (default) | "obfuscated_obfuscar"
#                       | "obfuscated_eazfuscator" | "obfuscated_reactor"
# Output:      results/<AppName>/<version>.json
#
# Bachelor's Thesis: Analysis, Methodology and Experimental Evaluation
#                    of IL Code Obfuscation Techniques in .NET Applications
# Year:        2026
#============================================================================

$root = Split-Path $PSScriptRoot -Parent
$analyzer = "$root/src/Analyzer/Analyzer.csproj"

$projects = @(
    @{ Name = "CliApp";       Dll = "CliApp.dll" },
    @{ Name = "AlgorithmLib"; Dll = "AlgorithmLib.dll" },
    @{ Name = "ClassLibrary"; Dll = "ClassLibrary.dll" },
    @{ Name = "WebApi";       Dll = "WebApi.dll" },
    @{ Name = "DesktopApp";   Dll = "DesktopApp.dll" }
)

# Build variant to analyze; defaults to the reference build.
$version = if ($args[0]) { $args[0] } else { "reference" }

Write-Host "Analyzing version: $version" -ForegroundColor Cyan

foreach ($project in $projects) {
    $dll = "$root/results/$($project.Name)/$version/$($project.Dll)"
    $output = "$root/results/$($project.Name)/$version.json"

    if (-not (Test-Path $dll)) {
        Write-Host "SKIP $($project.Name) - DLL not found: $dll" -ForegroundColor Yellow
        continue
    }

    # Invoke the Analyzer tool. Output is suppressed because the JSON
    # file produced on disk is the canonical result.
    dotnet run --project $analyzer -c Release -- $dll $output | Out-Null

    $json = Get-Content $output | ConvertFrom-Json
    Write-Host "OK $($project.Name): types=$($json.TypeCount) methods=$($json.MethodCount) il=$($json.ILInstructionCount) strings=$($json.StringLiteralCount)" -ForegroundColor Green
}

Write-Host "Done." -ForegroundColor Yellow
