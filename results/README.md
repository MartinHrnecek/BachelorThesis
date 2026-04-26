# Experimental results

This directory contains the archived measurements presented in Chapter 4 of
the thesis. The files are kept under version control so that reviewers can
verify the numbers in the thesis tables without having to re-run the full
pipeline (which requires commercial obfuscator licenses for full coverage).

## Top-level files

| File                          | Description                                                          |
|-------------------------------|----------------------------------------------------------------------|
| `metrics_all.csv`             | All metrics for 15 application × obfuscator combinations             |
| `benchmark_all.csv`           | Aggregated performance summary across the four AlgorithmLib variants |
| `benchmark_reference.csv`     | BenchmarkDotNet raw output for the non-obfuscated baseline           |
| `benchmark_obfuscated_obfuscar.csv`    | BDN output for the Obfuscar build                           |
| `benchmark_obfuscated_eazfuscator.csv` | BDN output for the Eazfuscator.NET build                    |
| `benchmark_obfuscated_reactor.csv`     | BDN output for the .NET Reactor build                       |

## Per-application directories

Each application (`CliApp/`, `AlgorithmLib/`, `ClassLibrary/`, `WebApi/`,
`DesktopApp/`) contains:

| File                            | Description                                              |
|---------------------------------|----------------------------------------------------------|
| `reference.json`                | Detailed metrics of the non-obfuscated reference build   |
| `obfuscated_obfuscar.json`      | Detailed metrics after Obfuscar obfuscation              |
| `obfuscated_eazfuscator.json`   | Detailed metrics after Eazfuscator.NET obfuscation       |
| `obfuscated_reactor.json`       | Detailed metrics after .NET Reactor obfuscation          |
| `deobfuscation_obfuscar.json`   | Deobfuscation score (S_deobf) for the Obfuscar build     |
| `deobfuscation_eazfuscator.json`| Deobfuscation score for the Eazfuscator build            |
| `deobfuscation_reactor.json`    | Deobfuscation score for the .NET Reactor build           |

## What is excluded from version control

The following pipeline outputs are **not** committed (see `.gitignore`):

- `<App>/reference/*.dll` — built assemblies (regenerable via the pipeline)
- `<App>/obfuscated_*/*.dll` — obfuscated assemblies (commercial license issue)
- `<App>/decompiled_*/*.cs` — ILSpy decompilation output (large, regenerable)
- `<App>/obfuscar_config.xml` — auto-generated per-app Obfuscar config

## Reproducing the data

To regenerate this entire directory from scratch:

```powershell
# Clear existing data
Remove-Item -Recurse -Force results/* -ErrorAction SilentlyContinue

# Run the pipeline
Set-ExecutionPolicy -Scope Process -ExecutionPolicy Bypass
.\scripts\run_all.ps1
```

The pipeline will skip commercial obfuscators automatically if their binaries
are not installed, so partial reproduction (Obfuscar only) is also possible
without any commercial licenses.

## Measurement environment

These measurements were obtained on:

- **CPU:** AMD Ryzen 5 2600 @ 3.40 GHz
- **OS:** Windows 11 23H2
- **Runtime:** .NET 9.0.9 (X64 RyuJIT)
- **BenchmarkDotNet:** v0.15.8

Performance measurements (`benchmark_*.csv`) are inherently
hardware-dependent; the reproduced numbers may differ on different CPUs,
but the relative ratios between obfuscators should remain consistent.
Structural metrics (`metrics_all.csv`) are deterministic and should
reproduce identically across machines, modulo minor variations introduced
by different obfuscator versions.