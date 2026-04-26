# Analysis, Methodology and Experimental Evaluation of IL Code Obfuscation Techniques in .NET Applications

**Bachelor's Thesis** — Faculty of Information Technology, Brno University of Technology (FIT VUT)

**Author:** Martin Hrneček ([xhrnecm00](mailto:xhrnecm00@stud.fit.vutbr.cz))
**Supervisor:** Ing. Jan Pluskal, Ph.D.
**Department:** Department of Information Systems (UIFS)
**Year:** 2025/2026

---

## About

This repository contains the source code, analysis tools, automation scripts
and experimental results that accompany the bachelor's thesis on IL code
obfuscation in .NET applications. The thesis evaluates three obfuscators
(Obfuscar, Eazfuscator.NET, .NET Reactor) on five test applications using a
custom metrics framework based on Halstead complexity metrics, cyclomatic
complexity, IL instruction count, Shannon entropy of symbol names, and a
custom deobfuscation score derived from ILSpy decompilation output.

## Repository structure

```
BachelorThesis/
├── src/                        Source code
│   ├── AlgorithmLib/           Compute-bound algorithms (test app)
│   ├── AlgorithmLib.Interfaces/ Public contracts of AlgorithmLib
│   ├── Analyzer/               IL metrics extractor (dnlib-based)
│   ├── BenchmarkRunner/        Reflection-based BDN harness
│   ├── ClassLibrary/           Generic library (test app)
│   ├── CliApp/                 Console application (test app)
│   ├── DeobfuscationScorer/    S_deobf calculator (ILSpy diff)
│   ├── DesktopApp/             WPF application (test app)
│   └── WebApi/                 ASP.NET Core API (test app)
├── scripts/                    PowerShell automation
│   ├── run_all.ps1             Master pipeline (recommended entry point)
│   ├── 01_build_reference.ps1  Build all reference versions
│   ├── 02_obfuscate.ps1        Apply selected obfuscator
│   ├── 04_analyze.ps1          Run Analyzer on a build variant
│   └── 05_compare.ps1          Compute ratio metrics
├── tools/                      Obfuscator integration
│   ├── obfuscar/               Obfuscar XML configuration template
│   ├── eazfuscator/            Eazfuscator setup notes
│   └── reactor/                .NET Reactor setup notes
├── results/                    Experimental measurements (CSV/JSON)
├── README.md                   This file
├── LICENSE                     MIT
└── .gitignore
```

## Test applications

Five applications were chosen to cover a representative set of .NET project
types and language features:

| Project        | Type                  | Highlights                                               |
|----------------|-----------------------|----------------------------------------------------------|
| `CliApp`       | Console application   | Reflection, async/await, exception handling, generics    |
| `AlgorithmLib` | Class library         | Pure compute workloads (sorts, search, math, strings)    |
| `ClassLibrary` | Class library         | Generics, lambdas, extension methods, event aggregator   |
| `WebApi`       | ASP.NET Core Web API  | DI, attribute routing, model binding                     |
| `DesktopApp`   | WPF desktop app       | XAML, MVVM-lite, history service                         |

## Custom analysis tools

| Tool                  | Purpose                                                                                |
|-----------------------|----------------------------------------------------------------------------------------|
| `Analyzer`            | Extracts structural and complexity metrics directly from IL using dnlib                |
| `DeobfuscationScorer` | Computes S_deobf by diffing decompiled C# output of reference vs obfuscated builds     |
| `BenchmarkRunner`     | Reflection-based BenchmarkDotNet harness that works against renamed obfuscated symbols |

## Reproducing the experiments

### Prerequisites

Required (open-source, free):

- **.NET SDK 9.0 or newer** — [download](https://dotnet.microsoft.com/download)
- **Obfuscar** — `dotnet tool install -g Obfuscar.GlobalTool`
- **ilspycmd** — `dotnet tool install -g ilspycmd`

Optional (commercial — measurements for these tools will be skipped if absent):

- **Eazfuscator.NET 2025.3** (trial license) — see `tools/eazfuscator/README.md`
- **.NET Reactor 6.9** (demo license) — see `tools/reactor/README.md`

### Running the pipeline

From the repository root, in PowerShell:

```powershell
# Allow script execution for this session (Windows Defender SmartScreen)
Set-ExecutionPolicy -Scope Process -ExecutionPolicy Bypass

# Run the full pipeline
.\scripts\run_all.ps1
```

The pipeline performs the following steps:

1. **Build reference** — publishes Release builds of all five test apps
2. **Analyze reference** — extracts baseline metrics
3. **Obfuscate** — applies each available obfuscator
4. **Analyze obfuscated** — computes ratio metrics (M_IL, M_strings, etc.)
5. **Decompile + score** — runs ilspycmd and computes S_deobf
6. **Benchmark** — measures performance overhead via BenchmarkDotNet

Total runtime is approximately **10-15 minutes** on a typical desktop CPU.

### Pipeline options

```powershell
.\scripts\run_all.ps1 -SkipBenchmarks      # Skip step 6 (~5 min faster)
.\scripts\run_all.ps1 -SkipObfuscation     # Re-analyze existing builds
.\scripts\run_all.ps1 -SkipDeobfuscation   # Skip step 5
```

### Output files

After successful execution, `results/` contains:

| File                       | Content                                                            |
|----------------------------|--------------------------------------------------------------------|
| `metrics_all.csv`          | Aggregated metrics for all 15 application × obfuscator combinations|
| `benchmark_all.csv`        | Performance summary across the four AlgorithmLib build variants    |
| `benchmark_<variant>.csv`  | Per-variant raw BenchmarkDotNet output                             |
| `<App>/reference.json`     | Baseline metrics for the application                               |
| `<App>/obfuscated_*.json`  | Per-obfuscator metrics                                             |
| `<App>/deobfuscation_*.json` | Per-obfuscator deobfuscation score                              |

## Hardware and software environment

The measurements presented in the thesis were obtained on:

- **CPU:** AMD Ryzen 5 2600 @ 3.40 GHz, 6 physical / 12 logical cores
- **OS:** Windows 11 (10.0.22631, 23H2)
- **Runtime:** .NET SDK 9.0.305, .NET 9.0.9 (X64 RyuJIT x86-64-v3)
- **BenchmarkDotNet:** v0.15.8

## License

This source code is released under the [MIT License](LICENSE).

Third-party tools referenced by the pipeline (Obfuscar, Eazfuscator.NET,
.NET Reactor, ILSpy, BenchmarkDotNet, dnlib) are governed by their own
licenses; see the LICENSE file for details and the per-tool README files in
`tools/` for installation instructions.