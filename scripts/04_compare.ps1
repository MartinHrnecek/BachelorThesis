#============================================================================
# File:        04_compare.ps1
# Project:     ObfuscationStudy pipeline
# Author:      Martin Hrnecek <xhrnecm00>
# Description: Computes the ratio metrics M_types, M_methods, M_IL and
#              M_strings (as defined in the methodology, Section 3.4.1)
#              by comparing the reference and obfuscated analysis JSON
#              files of every test application. Results are printed to
#              the console and exported to CSV for further processing.
#
# Usage:       .\04_compare.ps1 [obfuscator]
#              obfuscator: "obfuscar" (default) | "eazfuscator" | "reactor"
# Output:      results/metrics_<obfuscator>.csv
#
# Bachelor's Thesis: Analysis, Methodology and Experimental Evaluation
#                    of IL Code Obfuscation Techniques in .NET Applications
# Year:        2026
#============================================================================

$root = Split-Path $PSScriptRoot -Parent

$projects = @("CliApp", "AlgorithmLib", "ClassLibrary", "WebApi", "DesktopApp")
$obfuscator = if ($args[0]) { $args[0] } else { "obfuscar" }

Write-Host "Comparing reference vs obfuscated_$obfuscator" -ForegroundColor Cyan
Write-Host ("{0,-15} {1,8} {2,8} {3,8} {4,8}" -f "App", "M_types", "M_methods", "M_IL", "M_strings")
Write-Host ("-" * 55)

$results = @()

foreach ($name in $projects) {
    $refFile = "$root/results/$name/reference.json"
    $obfFile = "$root/results/$name/obfuscated_$obfuscator.json"

    if (-not (Test-Path $refFile) -or -not (Test-Path $obfFile)) {
        Write-Host "SKIP $name - missing JSON" -ForegroundColor Yellow
        continue
    }

    $ref = Get-Content $refFile | ConvertFrom-Json
    $obf = Get-Content $obfFile | ConvertFrom-Json

    # Ratio metrics: M_x = N_x(obfuscated) / N_x(reference). A value of
    # 1.0 indicates no change; values below 1.0 indicate removal of the
    # corresponding artefacts (e.g. typical for M_strings under string
    # encryption); values above 1.0 indicate inflation.
    $mTypes   = [math]::Round($obf.TypeCount / $ref.TypeCount, 3)
    $mMethods = [math]::Round($obf.MethodCount / $ref.MethodCount, 3)
    $mIL      = [math]::Round($obf.ILInstructionCount / $ref.ILInstructionCount, 3)
    $mStrings = if ($ref.StringLiteralCount -eq 0) { "N/A" } else { [math]::Round($obf.StringLiteralCount / $ref.StringLiteralCount, 3) }

    Write-Host ("{0,-15} {1,8} {2,8} {3,8} {4,8}" -f $name, $mTypes, $mMethods, $mIL, $mStrings)

    $results += [PSCustomObject]@{
        App       = $name
        M_types   = $mTypes
        M_methods = $mMethods
        M_IL      = $mIL
        M_strings = $mStrings
    }
}

$results | Export-Csv "$root/results/metrics_$obfuscator.csv" -NoTypeInformation -Encoding UTF8
Write-Host "`nSaved to results/metrics_$obfuscator.csv" -ForegroundColor Yellow
