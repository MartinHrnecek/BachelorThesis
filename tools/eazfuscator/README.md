# Eazfuscator.NET integration

Eazfuscator.NET is a commercial obfuscator from Gapotchenko. This directory
**does not include any binaries or license keys** — users must obtain a trial
or commercial license themselves before running the obfuscation pipeline.

## Installation

1. Download Eazfuscator.NET from
   [https://www.gapotchenko.com/eazfuscator.net](https://www.gapotchenko.com/eazfuscator.net)
2. Run the installer (default install path on Windows is
   `C:\Program Files (x86)\Gapotchenko\Eazfuscator.NET\`)
3. Activate trial or commercial license
4. The pipeline (`scripts\run_all.ps1`) auto-detects the standard install
   location; if you install elsewhere, edit the `$eaz` variable in
   `run_all.ps1`.

## Configuration used in the experiments

The thesis uses the **default Eazfuscator.NET configuration** without
activation of advanced features:

- Symbol renaming: enabled (default)
- String encryption: enabled (default)
- Anti-tamper: disabled
- Anti-debug: disabled
- Control flow obfuscation: disabled

The CLI is invoked with only the input/output paths:

```powershell
& Eazfuscator.NET.exe <input.dll> -o <output_dir>
```

## Version used in experiments

**Eazfuscator.NET 2025.3** (trial license).

If you use a different version, the measured metrics (especially
`CC_max = 55`, the constant fingerprint of the Eazfuscator state-machine
runtime) may differ slightly.

## License notice

Eazfuscator.NET is **commercial software** licensed by Gapotchenko Labs LLC.
This repository contains **no Eazfuscator.NET binaries, configuration files,
or license keys**. Users must obtain their own trial or commercial license
to reproduce the experiments.

## Pipeline behaviour without Eazfuscator

If Eazfuscator.NET is not installed when `run_all.ps1` is executed, the
pipeline emits a soft warning during the prerequisite check and skips the
Eazfuscator obfuscation step automatically. Other obfuscators continue
unaffected.