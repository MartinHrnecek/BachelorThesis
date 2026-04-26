#============================================================================
# File:        run_all.ps1
# Project:     ObfuscationStudy pipeline
# Author:      Martin Hrnecek <xhrnecm00>
# Description: Master orchestration script that drives the full
#              experimental pipeline end-to-end:
#                [1] Build reference (non-obfuscated) versions
#                [2] Analyze reference versions (extract baseline metrics)
#                [3] Apply each obfuscator to every reference build
#                [4] Analyze obfuscated versions and compute ratio metrics
#                [5] Decompile both variants with ILSpy and score deobfuscation
#                [6] Run BenchmarkDotNet performance measurements
#
#              Before any work is done the script verifies that all
#              required external tools are present on the system and
#              prints download instructions for any missing ones, so
#              that reviewers (supervisor, opponent) can quickly see
#              what is needed to reproduce the experiments.
#
# Usage:       .\run_all.ps1
#                  [-SkipBenchmarks]    skip step [6]
#                  [-SkipObfuscation]   skip step [3] (use existing builds)
#                  [-SkipDeobfuscation] skip step [5]
#
# Output:      results/metrics_all.csv    - aggregated metrics
#              results/benchmark_*.csv    - performance data
#              results/<App>/*.json       - per-application JSON outputs
#
# Prerequisites:
#   - .NET SDK 9.0 or newer       https://dotnet.microsoft.com/download
#   - Obfuscar (NuGet global tool)
#       dotnet tool install -g Obfuscar.GlobalTool
#   - ilspycmd (NuGet global tool)
#       dotnet tool install -g ilspycmd
#   - Eazfuscator.NET (commercial trial)
#       https://www.gapotchenko.com/eazfuscator.net
#   - .NET Reactor (commercial demo)
#       https://www.eziriz.com/dotnet_reactor.htm
#
# Bachelor's Thesis: Analysis, Methodology and Experimental Evaluation
#                    of IL Code Obfuscation Techniques in .NET Applications
# Year:        2026
#============================================================================

param(
    [switch]$SkipBenchmarks,
    [switch]$SkipObfuscation,
    [switch]$SkipDeobfuscation
)

$root = Split-Path $PSScriptRoot -Parent
$startTime = Get-Date

# ---------------------------------------------------------------------------
# Prerequisite check
# ---------------------------------------------------------------------------
# Verifies that every required tool is installed before any work is done.
# Each missing tool is reported with a short description and a direct
# installation command or download URL so that reviewers can resolve the
# issue without searching through documentation.

function Test-Prerequisites {
    param([switch]$SkipObfuscation, [switch]$SkipDeobfuscation)

    $missing = @()
    $warnings = @()

    Write-Host ""
    Write-Host "Checking prerequisites..." -ForegroundColor Cyan

    # --- .NET SDK ----------------------------------------------------------
    try {
        $dotnetVersion = & dotnet --version 2>$null
        if ($LASTEXITCODE -eq 0) {
            Write-Host "  [OK]   .NET SDK         $dotnetVersion" -ForegroundColor Green
        } else { throw }
    } catch {
        $missing += @{
            Name        = ".NET SDK 9.0 or newer"
            Description = "Required for building, analysis and benchmarking."
            Install     = "https://dotnet.microsoft.com/download"
        }
    }

    # --- Obfuscar (NuGet global tool) -------------------------------------
    if (-not $SkipObfuscation) {
        $obfuscarOk = $false
        try {
            $null = & obfuscar.console --help 2>$null
            $obfuscarOk = ($LASTEXITCODE -eq 0 -or $LASTEXITCODE -eq 1)
        } catch { $obfuscarOk = $false }

        if ($obfuscarOk) {
            Write-Host "  [OK]   Obfuscar         (global tool)" -ForegroundColor Green
        } else {
            $missing += @{
                Name        = "Obfuscar"
                Description = "Open-source obfuscator (used in step [3])."
                Install     = "dotnet tool install -g Obfuscar.GlobalTool"
            }
        }
    }

    # --- ilspycmd (NuGet global tool) -------------------------------------
    if (-not $SkipDeobfuscation) {
        $ilspyOk = $false
        try {
            $null = & ilspycmd --help 2>$null
            $ilspyOk = ($LASTEXITCODE -eq 0 -or $LASTEXITCODE -eq 1)
        } catch { $ilspyOk = $false }

        if ($ilspyOk) {
            Write-Host "  [OK]   ilspycmd         (global tool)" -ForegroundColor Green
        } else {
            $missing += @{
                Name        = "ilspycmd"
                Description = "ILSpy command-line decompiler (used in step [5])."
                Install     = "dotnet tool install -g ilspycmd"
            }
        }
    }

    # --- Eazfuscator.NET (commercial) -------------------------------------
    if (-not $SkipObfuscation) {
        $eaz = "C:\Program Files (x86)\Gapotchenko\Eazfuscator.NET\Eazfuscator.NET.exe"
        if (Test-Path $eaz) {
            Write-Host "  [OK]   Eazfuscator.NET  $eaz" -ForegroundColor Green
        } else {
            $warnings += @{
                Name        = "Eazfuscator.NET"
                Description = "Commercial obfuscator (trial license required)."
                Install     = "https://www.gapotchenko.com/eazfuscator.net"
                Path        = $eaz
            }
        }
    }

    # --- .NET Reactor (commercial) ----------------------------------------
    if (-not $SkipObfuscation) {
        $reactor = "C:\Program Files (x86)\Eziriz\.NET Reactor\dotNET_Reactor.Console.exe"
        if (Test-Path $reactor) {
            Write-Host "  [OK]   .NET Reactor     $reactor" -ForegroundColor Green
        } else {
            $warnings += @{
                Name        = ".NET Reactor"
                Description = "Commercial obfuscator (demo license required)."
                Install     = "https://www.eziriz.com/dotnet_reactor.htm"
                Path        = $reactor
            }
        }
    }

    # --- Report missing required tools (hard stop) -----------------------
    if ($missing.Count -gt 0) {
        Write-Host ""
        Write-Host "[ERROR] Required tools are missing:" -ForegroundColor Red
        foreach ($m in $missing) {
            Write-Host ""
            Write-Host "  $($m.Name)" -ForegroundColor Red
            Write-Host "      $($m.Description)" -ForegroundColor Gray
            Write-Host "      Install: $($m.Install)" -ForegroundColor Yellow
        }
        Write-Host ""
        Write-Host "Pipeline aborted. Please install the missing tools and try again." -ForegroundColor Red
        return $false
    }

    # --- Report missing commercial tools (soft warning) ------------------
    if ($warnings.Count -gt 0) {
        Write-Host ""
        Write-Host "[WARNING] Commercial obfuscators not found:" -ForegroundColor Yellow
        foreach ($w in $warnings) {
            Write-Host ""
            Write-Host "  $($w.Name)" -ForegroundColor Yellow
            Write-Host "      $($w.Description)" -ForegroundColor Gray
            Write-Host "      Expected at: $($w.Path)" -ForegroundColor Gray
            Write-Host "      Download:    $($w.Install)" -ForegroundColor Gray
        }
        Write-Host ""
        Write-Host "These obfuscators will be skipped automatically." -ForegroundColor Yellow
        Write-Host "The pipeline will continue with the available tools." -ForegroundColor Yellow
        Write-Host ""
        Start-Sleep -Seconds 2
    }

    return $true
}

# ---------------------------------------------------------------------------
# Banner
# ---------------------------------------------------------------------------

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  IL Obfuscation Benchmark Pipeline" -ForegroundColor Cyan
Write-Host "  $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')" -ForegroundColor Cyan
Write-Host "  .NET SDK: $(dotnet --version 2>$null)" -ForegroundColor Cyan
Write-Host "  Machine: $env:COMPUTERNAME" -ForegroundColor Cyan
Write-Host "  OS: $([System.Environment]::OSVersion.VersionString)" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan

# Run prerequisite check; abort if required tools are missing.
if (-not (Test-Prerequisites -SkipObfuscation:$SkipObfuscation -SkipDeobfuscation:$SkipDeobfuscation)) {
    exit 1
}

# ---------------------------------------------------------------------------
# Configuration
# ---------------------------------------------------------------------------

# Test applications and their primary assembly names.
$projects = @(
    @{ Name = "CliApp";       Dll = "CliApp.dll" },
    @{ Name = "AlgorithmLib"; Dll = "AlgorithmLib.dll" },
    @{ Name = "ClassLibrary"; Dll = "ClassLibrary.dll" },
    @{ Name = "WebApi";       Dll = "WebApi.dll" },
    @{ Name = "DesktopApp";   Dll = "DesktopApp.dll" }
)

# Obfuscators evaluated by this pipeline. Filtered later based on which
# tools are actually installed (commercial obfuscators are optional).
$obfuscators = @("obfuscar", "eazfuscator", "reactor")

# Custom analysis tools authored as part of the thesis.
$analyzer            = "$root/src/Analyzer/Analyzer.csproj"
$scorer              = "$root/src/DeobfuscationScorer/DeobfuscationScorer.csproj"
$benchmarkRunner     = "$root/src/BenchmarkRunner/BenchmarkRunner.csproj"
$benchmarkRunnerDir  = "$root/src/BenchmarkRunner"

# Commercial obfuscator executable paths (default install locations).
$eaz     = "C:\Program Files (x86)\Gapotchenko\Eazfuscator.NET\Eazfuscator.NET.exe"
$reactor = "C:\Program Files (x86)\Eziriz\.NET Reactor\dotNET_Reactor.Console.exe"

# ---------------------------------------------------------------------------
# [1/6] Build reference (non-obfuscated) versions
# ---------------------------------------------------------------------------

Write-Host ""
Write-Host "[1/6] Building reference versions..." -ForegroundColor Yellow
foreach ($p in $projects) {
    $out = "$root/results/$($p.Name)/reference"
    dotnet publish "$root/src/$($p.Name)/$($p.Name).csproj" -c Release -o $out --nologo -v q 2>&1 | Out-Null
    Write-Host "  OK $($p.Name)" -ForegroundColor Green
}

# ---------------------------------------------------------------------------
# [2/6] Analyze reference versions
# ---------------------------------------------------------------------------
# Establishes the baseline metrics that all obfuscated variants are
# compared against.

Write-Host ""
Write-Host "[2/6] Analyzing reference versions..." -ForegroundColor Yellow
foreach ($p in $projects) {
    $dll  = "$root/results/$($p.Name)/reference/$($p.Dll)"
    $json = "$root/results/$($p.Name)/reference.json"
    dotnet run --project $analyzer -c Release -- $dll $json 2>&1 | Out-Null
    $data = Get-Content $json | ConvertFrom-Json
    Write-Host "  $($p.Name): types=$($data.TypeCount) methods=$($data.MethodCount) il=$($data.ILInstructionCount) vol=$($data.Halstead.Volume) diff=$($data.Halstead.Difficulty) cc=$($data.CyclomaticComplexity.Average)" -ForegroundColor Green
}

# ---------------------------------------------------------------------------
# [3/6] Obfuscate
# ---------------------------------------------------------------------------
# Applies each obfuscator to every reference build, skipping commercial
# obfuscators that were detected as missing during the prerequisite check.
# After each obfuscator runs, missing dependencies (e.g. referenced
# libraries that the obfuscator did not copy) are filled in from the
# reference build so that the obfuscated assembly remains loadable.

if (-not $SkipObfuscation) {
    Write-Host ""
    Write-Host "[3/6] Obfuscating..." -ForegroundColor Yellow

    function Copy-MissingDependencies {
        param([string]$RefDir, [string]$OutDir)
        Get-ChildItem $RefDir -File | ForEach-Object {
            $dest = Join-Path $OutDir $_.Name
            if (-not (Test-Path $dest)) {
                Copy-Item $_.FullName $dest -Force
            }
        }
    }

    foreach ($p in $projects) {
        $ref = "$root/results/$($p.Name)/reference"

        # --- Obfuscar (open-source) ----------------------------------------
        # Generate a per-application Obfuscar config and invoke the CLI.
        $outObf = "$root/results/$($p.Name)/obfuscated_obfuscar"
        New-Item $outObf -ItemType Directory -Force | Out-Null
        $config = @"
<?xml version="1.0" encoding="UTF-8"?>
<Obfuscator>
  <Var name="InPath" value="$ref" />
  <Var name="OutPath" value="$outObf" />
  <Var name="KeepPublicApi" value="false" />
  <Var name="HidePrivateApi" value="true" />
  <Var name="RenameFields" value="true" />
  <Var name="RenameProperties" value="true" />
  <Var name="RenameEvents" value="true" />
  <Var name="ObfuscateAssembly" value="true" />
  <Module file="$ref/$($p.Dll)" />
</Obfuscator>
"@
        $config | Set-Content "$root/results/$($p.Name)/obfuscar_config.xml" -Encoding UTF8
        obfuscar.console "$root/results/$($p.Name)/obfuscar_config.xml" 2>&1 | Out-Null
        Copy-MissingDependencies $ref $outObf
        Write-Host "  Obfuscar: $($p.Name) OK" -ForegroundColor Green

        # --- Eazfuscator.NET (commercial) ----------------------------------
        if (Test-Path $eaz) {
            $outEaz = "$root/results/$($p.Name)/obfuscated_eazfuscator"
            New-Item $outEaz -ItemType Directory -Force | Out-Null
            & $eaz "$ref/$($p.Dll)" -o $outEaz 2>&1 | Out-Null
            Copy-MissingDependencies $ref $outEaz
            Write-Host "  Eazfuscator: $($p.Name) OK" -ForegroundColor Green
        } else {
            Write-Host "  Eazfuscator: $($p.Name) SKIPPED (tool not installed)" -ForegroundColor DarkYellow
        }

        # --- .NET Reactor (commercial) -------------------------------------
        if (Test-Path $reactor) {
            $outReactor = "$root/results/$($p.Name)/obfuscated_reactor"
            New-Item $outReactor -ItemType Directory -Force | Out-Null
            & $reactor -file "$ref/$($p.Dll)" -targetfile "$outReactor/$($p.Dll)" -obfuscate 1 -control_flow 1 -string_encryption 1 -rename_fields 1 -rename_methods 1 -rename_properties 1 2>&1 | Out-Null
            Copy-MissingDependencies $ref $outReactor
            Write-Host "  .NET Reactor: $($p.Name) OK" -ForegroundColor Green
        } else {
            Write-Host "  .NET Reactor: $($p.Name) SKIPPED (tool not installed)" -ForegroundColor DarkYellow
        }
    }
}

# ---------------------------------------------------------------------------
# [4/6] Analyze obfuscated versions and compute ratio metrics
# ---------------------------------------------------------------------------

Write-Host ""
Write-Host "[4/6] Analyzing obfuscated versions + binary sizes..." -ForegroundColor Yellow

$allMetrics = @()

foreach ($p in $projects) {
    $ref     = Get-Content "$root/results/$($p.Name)/reference.json" | ConvertFrom-Json
    $refSize = (Get-Item "$root/results/$($p.Name)/reference/$($p.Dll)").Length

    foreach ($obf in $obfuscators) {
        $dll  = "$root/results/$($p.Name)/obfuscated_$obf/$($p.Dll)"
        $json = "$root/results/$($p.Name)/obfuscated_$obf.json"

        if (-not (Test-Path $dll)) {
            Write-Host "  SKIP $($p.Name) / $obf" -ForegroundColor Yellow
            continue
        }

        # Run the Analyzer tool on the obfuscated assembly.
        dotnet run --project $analyzer -c Release -- $dll $json 2>&1 | Out-Null
        $d = Get-Content $json | ConvertFrom-Json

        # --- Reconstruction ratio metrics (Section 3.4.1) ------------------
        $mTypes   = [math]::Round($d.TypeCount / $ref.TypeCount, 3)
        $mMethods = [math]::Round($d.MethodCount / $ref.MethodCount, 3)
        $mIL      = [math]::Round($d.ILInstructionCount / $ref.ILInstructionCount, 3)
        $mStrings = if ($ref.StringLiteralCount -eq 0) { "N/A" } else {
            [math]::Round($d.StringLiteralCount / $ref.StringLiteralCount, 3)
        }
        $obfSize = (Get-Item $dll).Length
        $mSize   = [math]::Round(($obfSize - $refSize) / $refSize, 3)

        # --- Halstead complexity (Section 3.4.2) ---------------------------
        $halVol      = [math]::Round($d.Halstead.Volume, 1)
        $halDiff     = [math]::Round($d.Halstead.Difficulty, 1)
        $halEff      = [math]::Round($d.Halstead.Effort, 0)
        $ccAvg       = $d.CyclomaticComplexity.Average
        $ccMax       = $d.CyclomaticComplexity.Maximum
        $entType     = $d.SymbolEntropy.TypeNameEntropy
        $entMeth     = $d.SymbolEntropy.MethodNameEntropy

        $halVolRatio  = [math]::Round($halVol  / $ref.Halstead.Volume, 3)
        $halDiffRatio = [math]::Round($halDiff / $ref.Halstead.Difficulty, 3)
        $halEffRatio  = [math]::Round($halEff  / $ref.Halstead.Effort, 3)
        $ccRatio      = [math]::Round($ccAvg   / $ref.CyclomaticComplexity.Average, 3)

        $allMetrics += [PSCustomObject]@{
            App            = $p.Name
            Obfuscator     = $obf
            M_types        = $mTypes
            M_methods      = $mMethods
            M_IL           = $mIL
            M_strings      = $mStrings
            M_size         = $mSize
            Size_ref       = $refSize
            Size_obf       = $obfSize
            Hal_Volume     = $halVol
            Hal_Difficulty = $halDiff
            Hal_Effort     = $halEff
            Hal_Vol_ratio  = $halVolRatio
            Hal_Diff_ratio = $halDiffRatio
            Hal_Eff_ratio  = $halEffRatio
            CC_avg         = $ccAvg
            CC_max         = $ccMax
            CC_ratio       = $ccRatio
            Ent_type       = $entType
            Ent_method     = $entMeth
            Deobf_score    = "N/A"
        }

        Write-Host "  $($p.Name)/$obf M_IL=$mIL Hal_Diff=$halDiff CC_avg=$ccAvg" -ForegroundColor Green
    }
}

$allMetrics | Export-Csv "$root/results/metrics_all.csv" -NoTypeInformation -Encoding UTF8

# ---------------------------------------------------------------------------
# [5/6] Decompile + score deobfuscation
# ---------------------------------------------------------------------------
# For each application: decompile both reference and obfuscated builds
# with ilspycmd, then run the DeobfuscationScorer to compute S_deobf.

if (-not $SkipDeobfuscation) {
    Write-Host ""
    Write-Host "[5/6] Decompiling + scoring deobfuscation..." -ForegroundColor Yellow

    foreach ($p in $projects) {
        $refDecomp = "$root/results/$($p.Name)/decompiled_reference"
        ilspycmd "$root/results/$($p.Name)/reference/$($p.Dll)" -p -o $refDecomp 2>&1 | Out-Null

        foreach ($obf in $obfuscators) {
            $dll = "$root/results/$($p.Name)/obfuscated_$obf/$($p.Dll)"
            if (-not (Test-Path $dll)) { continue }

            $obfDecomp  = "$root/results/$($p.Name)/decompiled_$obf"
            $scoreJson  = "$root/results/$($p.Name)/deobfuscation_$obf.json"

            ilspycmd $dll -p -o $obfDecomp 2>&1 | Out-Null
            dotnet run --project $scorer -c Release -- $refDecomp $obfDecomp $scoreJson 2>&1 | Out-Null

            # Inject the deobfuscation score back into the in-memory metrics
            # so that the final CSV contains everything in a single row.
            $score = (Get-Content $scoreJson | ConvertFrom-Json).DeobfuscationScore
            $metric = $allMetrics | Where-Object { $_.App -eq $p.Name -and $_.Obfuscator -eq $obf }
            if ($metric) { $metric.Deobf_score = $score }

            Write-Host "  $($p.Name)/$obf deobf_score=$score" -ForegroundColor Green
        }
    }

    $allMetrics | Export-Csv "$root/results/metrics_all.csv" -NoTypeInformation -Encoding UTF8
}

# ---------------------------------------------------------------------------
# [6/6] BenchmarkDotNet performance measurements
# ---------------------------------------------------------------------------
# Performance comparison is performed only for AlgorithmLib because its
# methods are pure compute workloads suitable for microbenchmarks.

$benchResults = @()

if (-not $SkipBenchmarks) {
    Write-Host ""
    Write-Host "[6/6] Running benchmarks for AlgorithmLib..." -ForegroundColor Yellow

    New-Item "$root/results" -ItemType Directory -Force | Out-Null
    Remove-Item "$root/results/benchmark_*.csv" -ErrorAction SilentlyContinue

    $benchVersions = @(
        @{ Version = "reference";              Label = "Reference";    Dll = "$root/results/AlgorithmLib/reference/AlgorithmLib.dll" },
        @{ Version = "obfuscated_obfuscar";    Label = "Obfuscar";     Dll = "$root/results/AlgorithmLib/obfuscated_obfuscar/AlgorithmLib.dll" },
        @{ Version = "obfuscated_eazfuscator"; Label = "Eazfuscator";  Dll = "$root/results/AlgorithmLib/obfuscated_eazfuscator/AlgorithmLib.dll" },
        @{ Version = "obfuscated_reactor";     Label = ".NET Reactor"; Dll = "$root/results/AlgorithmLib/obfuscated_reactor/AlgorithmLib.dll" }
    )

    foreach ($bv in $benchVersions) {
        if (-not (Test-Path $bv.Dll)) {
            Write-Host "  SKIP $($bv.Label) - DLL not found" -ForegroundColor Yellow
            continue
        }

        Write-Host "  Benchmarking $($bv.Label)..." -ForegroundColor Cyan

        # Clear previous BenchmarkDotNet artefacts so that the latest run
        # is unambiguously identified by file timestamps.
        Remove-Item -Recurse -Force "$benchmarkRunnerDir/BenchmarkDotNet.Artifacts" -ErrorAction SilentlyContinue

        # Run the benchmark and wait for completion.
        Push-Location $benchmarkRunnerDir
        dotnet run --project $benchmarkRunner -c Release -- $bv.Dll
        Pop-Location

        # Collect the latest CSV report produced by BenchmarkDotNet.
        $artifactsPath = "$benchmarkRunnerDir/BenchmarkDotNet.Artifacts/results"
        $csv = Get-ChildItem $artifactsPath -Filter "*.csv" -ErrorAction SilentlyContinue | Sort-Object LastWriteTime | Select-Object -Last 1

        if ($csv) {
            $dest = "$root/results/benchmark_$($bv.Version).csv"
            Copy-Item $csv.FullName $dest -Force
            $rows = Import-Csv $dest -Delimiter ";"
            foreach ($row in $rows) {
                $benchResults += [PSCustomObject]@{
                    Obfuscator = $bv.Label
                    Method     = $row.Method
                    Mean_ns    = $row.Mean
                }
            }
            Write-Host "    OK -> $dest" -ForegroundColor Green
        } else {
            Write-Host "    WARNING: CSV not found in $artifactsPath" -ForegroundColor Red
        }
    }

    $benchResults | Export-Csv "$root/results/benchmark_all.csv" -NoTypeInformation -Encoding UTF8
} else {
    if (Test-Path "$root/results/benchmark_all.csv") {
        $benchResults = Import-Csv "$root/results/benchmark_all.csv"
    }
}

# ---------------------------------------------------------------------------
# Final summary
# ---------------------------------------------------------------------------

$elapsed = (Get-Date) - $startTime

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  RESULTS SUMMARY" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan

# --- Reconstruction metrics ------------------------------------------------
Write-Host ""
Write-Host "--- Reconstruction Metrics ---" -ForegroundColor White
Write-Host ("{0,-15} {1,-13} {2,8} {3,9} {4,8} {5,9} {6,8} {7,10}" -f "App", "Obfuscator", "M_types", "M_methods", "M_IL", "M_strings", "M_size", "Deobf_score")
Write-Host ("-" * 90)
foreach ($m in $allMetrics) {
    # Color-code the row based on M_IL severity for at-a-glance scanning.
    $color = if ($m.M_IL -gt 10) { "Red" } elseif ($m.M_IL -gt 3) { "Yellow" } else { "White" }
    Write-Host ("{0,-15} {1,-13} {2,8} {3,9} {4,8} {5,9} {6,8} {7,10}" -f $m.App, $m.Obfuscator, $m.M_types, $m.M_methods, $m.M_IL, $m.M_strings, $m.M_size, $m.Deobf_score) -ForegroundColor $color
}

# --- Halstead metrics ------------------------------------------------------
Write-Host ""
Write-Host "--- Halstead Complexity Metrics ---" -ForegroundColor White
Write-Host ("{0,-15} {1,-13} {2,10} {3,10} {4,12} {5,8} {6,8} {7,8}" -f "App", "Obfuscator", "Volume", "Difficulty", "Effort", "Vol_x", "Diff_x", "Eff_x")
Write-Host ("-" * 92)
foreach ($m in $allMetrics) {
    $color = if ($m.Hal_Diff_ratio -gt 3) { "Red" } elseif ($m.Hal_Diff_ratio -gt 1.5) { "Yellow" } else { "White" }
    Write-Host ("{0,-15} {1,-13} {2,10} {3,10} {4,12} {5,8} {6,8} {7,8}" -f $m.App, $m.Obfuscator, $m.Hal_Volume, $m.Hal_Difficulty, $m.Hal_Effort, $m.Hal_Vol_ratio, $m.Hal_Diff_ratio, $m.Hal_Eff_ratio) -ForegroundColor $color
}

# --- Cyclomatic complexity + symbol entropy --------------------------------
Write-Host ""
Write-Host "--- Cyclomatic Complexity + Symbol Entropy ---" -ForegroundColor White
Write-Host ("{0,-15} {1,-13} {2,10} {3,10} {4,10} {5,10} {6,10}" -f "App", "Obfuscator", "CC_avg", "CC_max", "CC_ratio", "Ent_type", "Ent_meth")
Write-Host ("-" * 82)
foreach ($m in $allMetrics) {
    $color = if ($m.CC_ratio -gt 3) { "Red" } elseif ($m.CC_ratio -gt 1.5) { "Yellow" } else { "White" }
    Write-Host ("{0,-15} {1,-13} {2,10} {3,10} {4,10} {5,10} {6,10}" -f $m.App, $m.Obfuscator, $m.CC_avg, $m.CC_max, $m.CC_ratio, $m.Ent_type, $m.Ent_method) -ForegroundColor $color
}

# --- Binary size summary ---------------------------------------------------
Write-Host ""
Write-Host "--- Binary Size Summary ---" -ForegroundColor White
Write-Host ("{0,-15} {1,-13} {2,12} {3,12}" -f "App", "Obfuscator", "Ref (B)", "Obf (B)")
Write-Host ("-" * 55)
foreach ($m in $allMetrics) {
    Write-Host ("{0,-15} {1,-13} {2,12} {3,12}" -f $m.App, $m.Obfuscator, $m.Size_ref, $m.Size_obf)
}

# --- Performance summary (AlgorithmLib only) -------------------------------
if ($benchResults.Count -gt 0) {
    Write-Host ""
    Write-Host "--- Performance Summary (AlgorithmLib) ---" -ForegroundColor White
    Write-Host ("{0,-22} {1,18} {2,18} {3,18} {4,18}" -f "Method", "Reference", "Obfuscar", "Eazfuscator", ".NET Reactor")
    Write-Host ("-" * 97)
    $methods = @("BubbleSort","QuickSort","Fibonacci","Factorial","BinarySearch","IsPalindrome","SieveOfEratosthenes")
    foreach ($method in $methods) {
        $ref = ($benchResults | Where-Object { $_.Obfuscator -eq "Reference"    -and $_.Method -eq $method }).Mean_ns
        $obf = ($benchResults | Where-Object { $_.Obfuscator -eq "Obfuscar"     -and $_.Method -eq $method }).Mean_ns
        $eaz = ($benchResults | Where-Object { $_.Obfuscator -eq "Eazfuscator"  -and $_.Method -eq $method }).Mean_ns
        $rea = ($benchResults | Where-Object { $_.Obfuscator -eq ".NET Reactor" -and $_.Method -eq $method }).Mean_ns
        Write-Host ("{0,-22} {1,18} {2,18} {3,18} {4,18}" -f $method, $ref, $obf, $eaz, $rea)
    }
}

# --- Output file index -----------------------------------------------------
Write-Host ""
Write-Host "--- Output Files ---" -ForegroundColor White
Write-Host "  results/metrics_all.csv     - all metrics"
Write-Host "  results/benchmark_all.csv   - performance (AlgorithmLib)"
Write-Host "  results/benchmark_*.csv     - per-version data"
Write-Host ""
Write-Host "Total time: $([math]::Round($elapsed.TotalMinutes, 1)) min" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan